using E_BookStore.DataAccess.Repository;
using E_BookStore.DataAccess.Repository.IRepository;
using E_BookStore.Models;
using E_BookStore.Models.ViewModels;
using E_BookStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace E_BookStoreWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _UnitOfWork;
        [BindProperty]
        public OrderVM orderVM { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            _UnitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int orderid)
        {
            orderVM = new() {
                OrderHeader = _UnitOfWork.OrderHeader.Get(u => u.Id == orderid,includeProperties:"ApplicationUser"),
                OrderDetail = _UnitOfWork.OrderDetail.GetAll(u=>u.OrderHeaderId==orderid, includeProperties: "Product")
            };
            return View(orderVM);
        }

        

        #region APICALLS
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> objOrderHeaders;
            
            if(User.IsInRole(SD.Role_Admin)|| User.IsInRole(SD.Role_Employee))
            {
                objOrderHeaders= _UnitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
            }
            else
            {   var claimsIdentity = (ClaimsIdentity)User.Identity;
                var user = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                objOrderHeaders = _UnitOfWork.OrderHeader.GetAll(u => u.ApplicationUserId == user,includeProperties:"ApplicationUser").ToList();
            }
            switch (status)
            {
                case "pending": objOrderHeaders = objOrderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment); break;
                case "inprocess": objOrderHeaders = objOrderHeaders.Where(u => u.PaymentStatus == SD.StatusInProcess); break;
                case "completed": objOrderHeaders = objOrderHeaders.Where(u => u.PaymentStatus == SD.StatusShipped); break;
                case "approved": objOrderHeaders = objOrderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusApproved); break;
                case "cancelled": objOrderHeaders = objOrderHeaders.Where(u => u.PaymentStatus == SD.StatusCancelled); break;
                default: break;
            }
            return Json(new { data = objOrderHeaders });
        }

        [HttpPost]
        [Authorize(Roles=SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult UpdateOrderDetails()
        {
            var orderHeaderFromDb = _UnitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id);
            orderHeaderFromDb.Name = orderVM.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = orderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = orderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = orderVM.OrderHeader.City;
            orderHeaderFromDb.State = orderVM.OrderHeader.State;
            orderHeaderFromDb.PostalCode = orderVM.OrderHeader.PostalCode;

            if (!string.IsNullOrEmpty(orderVM.OrderHeader.Carrier))
                orderHeaderFromDb.Carrier = orderVM.OrderHeader.Carrier;

            if (!string.IsNullOrEmpty(orderVM.OrderHeader.TrackingNumber))
                orderHeaderFromDb.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
            _UnitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _UnitOfWork.Save();
            return RedirectToAction(nameof(Details), new { orderid = orderHeaderFromDb.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing()
        {
            _UnitOfWork.OrderHeader.UpdateStatus(orderVM.OrderHeader.Id, SD.StatusInProcess);
            _UnitOfWork.Save();
            TempData["Success"] = "Order Details Updated Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles= SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {
            var orderHeader = _UnitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id);
            orderHeader.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier = orderVM.OrderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;

            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate =DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }
            _UnitOfWork.OrderHeader.Update(orderHeader);
            _UnitOfWork.Save();
            TempData["Success"] = "Order Shipped Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder()
        {
            var orderHeader = _UnitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id);

           

            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)  
            {
                //razorpay refund logic
            }
            _UnitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
            _UnitOfWork.OrderHeader.Update(orderHeader);
            _UnitOfWork.Save();
            TempData["Success"] = "Order Shipped Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id });
        }

        [ActionName("Details")]
        [HttpPost]
        public IActionResult Details_PAY_NOW() {
            orderVM.OrderHeader = _UnitOfWork.OrderHeader
                    .Get(u => u.Id == orderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
            orderVM.OrderDetail = _UnitOfWork.OrderDetail
                .GetAll(u => u.OrderHeaderId == orderVM.OrderHeader.Id, includeProperties: "Product");
            //razorpay payment logic here
            return new StatusCodeResult(303);
        }

        #endregion
    }
}