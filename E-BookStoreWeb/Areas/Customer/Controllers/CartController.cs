using E_BookStore.DataAccess.Repository.IRepository;
using E_BookStore.Models;
using E_BookStore.Models.ViewModels;
using E_BookStore.Utility;
using E_BookStore.Utility.RazorPay;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NuGet.Protocol;
using Razorpay.Api;
using System.Drawing;
using System.Security.Claims;

namespace E_BookStoreWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        public readonly IUnitOfWork _unitOfWork;
        private readonly RazorPaySettings _settings;
        [BindProperty]
        public ShoppingCartVM shoppingCartVM { get; set; }

        public CartController(IUnitOfWork unitOfWork,IOptions<RazorPaySettings> options)
        {
            _unitOfWork = unitOfWork;
            _settings = options.Value;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            shoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"),
                OrderHeader = new()
            };

            foreach (var item in shoppingCartVM.ShoppingCartList)
            {
                item.Price = GetPriceBasedOnQuantity(item);
                shoppingCartVM.OrderHeader.OrderTotal += (item.Price * item.Count);
            }

            return View(shoppingCartVM);

        }
        public IActionResult Summary() {

			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

			shoppingCartVM = new()
			{
				ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"),
				OrderHeader = new()

			};
            shoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u=>u.Id == userId);
			shoppingCartVM.OrderHeader.Name = shoppingCartVM.OrderHeader.ApplicationUser.Name;
			shoppingCartVM.OrderHeader.PhoneNumber = shoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
			shoppingCartVM.OrderHeader.StreetAddress = shoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
			shoppingCartVM.OrderHeader.City = shoppingCartVM.OrderHeader.ApplicationUser.City;
			shoppingCartVM.OrderHeader.State = shoppingCartVM.OrderHeader.ApplicationUser.State;
			shoppingCartVM.OrderHeader.PostalCode = shoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

			foreach (var item in shoppingCartVM.ShoppingCartList)
			{
				item.Price = GetPriceBasedOnQuantity(item);
				shoppingCartVM.OrderHeader.OrderTotal += (item.Price * item.Count);
			}
			return View(shoppingCartVM);
        }
        [HttpPost]
        [ActionName("Summary")]
		public IActionResult SummaryPOST()
		{

			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;// to get the user thats logged in

            shoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product");
            shoppingCartVM.OrderHeader.ApplicationUserId = userId;	
			ApplicationUser applicationUser= _unitOfWork.ApplicationUser.Get(u => u.Id == userId);// so that user table is not updated
			
			foreach (var item in shoppingCartVM.ShoppingCartList)
			{
				item.Price = GetPriceBasedOnQuantity(item);
				shoppingCartVM.OrderHeader.OrderTotal += (item.Price * item.Count);
			}
            //fill out orderheader
            if (applicationUser.CompanyId.GetValueOrDefault()==0) 
            {// regular customer
                shoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
                shoppingCartVM.OrderHeader.PaymentStatus = SD.StatusPending;
            }
            else
            {//company user
                shoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
                shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
            }
            _unitOfWork.OrderHeader.Add(shoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            // fill orderdetails
            foreach (var cart in shoppingCartVM.ShoppingCartList) {

				OrderDetail orderDetail = new()
				{
					OrderHeaderId = shoppingCartVM.OrderHeader.Id,
                    ProductId = cart.ProductId,
                    Price = cart.Price,
                    Count = cart.Count,
				};
                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
			}
			if (applicationUser.CompanyId.GetValueOrDefault() == 0)
			{// regular customer
             //get payment razorpay logic here
                RazorpayClient client = new RazorpayClient(_settings.KeyId,_settings.SecretKey);
                Dictionary<string, object> options = new Dictionary<string, object>();
                options.Add("amount", shoppingCartVM.OrderHeader.OrderTotal * 100); // amount in paise so multiplied by 100
                options.Add("currency", "INR");
                Order order = client.Order.Create(options);
                shoppingCartVM.OrderHeader.SessionId = order["id"].ToString();
                _unitOfWork.OrderHeader.Update(shoppingCartVM.OrderHeader);
                _unitOfWork.Save();

                return View("Payment",shoppingCartVM);
            }

			return RedirectToAction(nameof(OrderConfirmation),new { id = shoppingCartVM.OrderHeader.Id});
		}

        [HttpPost]
        public IActionResult PaymentCallback([FromQuery] string status)
        {
            var form = Request.Form;
            shoppingCartVM.OrderHeader = _unitOfWork.OrderHeader.Get(u => u.SessionId == form["razorpay_order_id"].ToString());
            if (status == "cancelled")
            {
                shoppingCartVM.OrderHeader.OrderStatus = SD.StatusCancelled;
                shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
                _unitOfWork.OrderHeader.Update(shoppingCartVM.OrderHeader);
                _unitOfWork.Save();
                return RedirectToAction(nameof(Summary));
            }
            else if (status == "success")
            {
                
                if (form != null)
                {
                   
                    shoppingCartVM.OrderHeader.PaymentIntentId = form["razorpay_payment_id"];
                    string razorpaySignature = form["razorpay_signature"];
                    shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
                    shoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
                    _unitOfWork.OrderHeader.Update(shoppingCartVM.OrderHeader);
                    _unitOfWork.Save();

                    

                    return RedirectToAction("Index", "Home", new { area = "Customer" });
                }
            }


            return RedirectToAction("Index", "Home", new { area = "Customer" });

        }

        public IActionResult OrderConfirmation(int id)
        {
            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == shoppingCartVM.OrderHeader.ApplicationUserId).ToList();

            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitOfWork.Save();
            return View(id);
        }

		public IActionResult Plus(int cartId) { 
        
            ShoppingCart cartFromDB = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId, includeProperties: "Product");

            cartFromDB.Count += 1;

            _unitOfWork.ShoppingCart.Update(cartFromDB);
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {

            ShoppingCart cartFromDB = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId, includeProperties: "Product");

            if (cartFromDB.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(cartFromDB);
            }
            else
            {
                cartFromDB.Count -= 1;
                _unitOfWork.ShoppingCart.Update(cartFromDB);
                
            }

            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {

            ShoppingCart cartFromDB = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId, includeProperties: "Product");

            _unitOfWork.ShoppingCart.Remove(cartFromDB);
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
            {
                return shoppingCart.Product.Price;
            }
            else
            {
                if (shoppingCart.Count <= 100)
                {
                    return shoppingCart.Product.Price50;
                }
                else
                {
                    return shoppingCart.Product.Price100;
                }
            }
        }
    }
}
