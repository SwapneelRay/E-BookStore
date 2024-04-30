using E_BookStore.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using E_BookStore.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using E_BookStore.Models.ViewModels;

namespace E_BookStoreWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        public readonly IUnitOfWork _UnitOfWork;
        public ProductController(IUnitOfWork unitOfWork)
        {
            _UnitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<Product> objProductList = _UnitOfWork.Product.GetAll().ToList();
            return View(objProductList);
        }
        /*public IActionResult Create()
        {
            IEnumerable<SelectListItem> CatergoryList = _UnitOfWork.Category.
                GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
            //ViewBag.CategoryList = CatergoryList;
            //ViewData["CategoryList"] = CatergoryList;
            ProductVM productVM = new()
            {
                CategoryList = _UnitOfWork.Category.
                GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Product = new Product()
            };
            return View(productVM);

        }*/
        /*[HttpPost]
        public IActionResult Create(ProductVM productVM)
        {
            *//* if(obj.Product.Price100 ==999 || obj.Product.Price100 == 10)
             {
                 ModelState.AddModelError("isbn", "isbn is invalid");
             }*//*

            if (ModelState.IsValid)
            {
                _UnitOfWork.Product.Add(productVM.Product);
                _UnitOfWork.Save();
                TempData["Success"] = "New Product has been added successfully";
                return RedirectToAction("Index");
            }
            else
            {
                productVM.CategoryList = _UnitOfWork.Category.
                GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });


                return View(productVM);
            }
        }*/

        public IActionResult Upsert(int? id) // update and insert in one
        {

            //ViewBag.CategoryList = CatergoryList;
            //ViewData["CategoryList"] = CatergoryList;
            ProductVM productVM = new()
            {
                CategoryList = _UnitOfWork.Category.
                GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Product = new Product()
            };
            if(id== null || id==0)
            {
                //create
                return View(productVM); 
            }
            else
            {
                //update
                productVM.Product = _UnitOfWork.Product.Get(u=>u.Id==id);
                return View(productVM);
            }
            
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file)
        {
           /* if(obj.Product.Price100 ==999 || obj.Product.Price100 == 10)
            {
                ModelState.AddModelError("isbn", "isbn is invalid");
            }*/

            if (ModelState.IsValid)
            {
                _UnitOfWork.Product.Add(productVM.Product);
                _UnitOfWork.Save();
                TempData["Success"] = "New Product has been added successfully";
                return RedirectToAction("Index");
            }
            else
            {
                productVM.CategoryList = _UnitOfWork.Category.
                GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
                

                return View(productVM);
            }
        }
            
        /* not need any more 
         public IActionResult Edit(int? id)
        {
            if(id == null || id == 0)
            {
                return NotFound();
            }
            Product? productFromDB = _UnitOfWork.Product.Get(x=>x.Id == id);
            return View(productFromDB);
        }
        [HttpPost]
        public IActionResult Edit(ProductVM obj)
        {
            if (ModelState.IsValid)
            {
                _UnitOfWork.Product.Update(obj.Product);
                _UnitOfWork.Save();
                TempData["Success"] = "The Product has been updated successfully";
                return RedirectToAction("Index");
            }
            return View();
        }*/

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Product? productFromDB = _UnitOfWork.Product.Get(x => x.Id == id);
            return View(productFromDB);
        }
        [HttpPost,ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            Product? obj = _UnitOfWork.Product.Get(x=> x.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            
            _UnitOfWork.Product.Remove(obj);
            _UnitOfWork.Save();
            TempData["Success"] = "The Product has been deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
