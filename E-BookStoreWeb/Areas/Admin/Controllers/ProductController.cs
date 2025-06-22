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
        public readonly IWebHostEnvironment _WebHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork,IWebHostEnvironment webHostEnvironment)
        {
            _UnitOfWork = unitOfWork;
            _WebHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<Product> objProductList = _UnitOfWork.Product.GetAll(includeProperties:"Category").ToList();
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
                string wwwRootPath = _WebHostEnvironment.WebRootPath;
                if(file != null)
                {
                    string filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\product");

                    if(!string.IsNullOrEmpty(productVM.Product.ImageURL))
                    {
                        //delete old image
                        var oldImagePath = Path.Combine(wwwRootPath,productVM.Product.ImageURL.TrimStart('\\'));
                        if(System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(productPath, filename), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    productVM.Product.ImageURL = @"\images\product\" + filename;
                }
                if(productVM.Product.Id==0)
                {
                    _UnitOfWork.Product.Add(productVM.Product);
                }
                else
                {
                    _UnitOfWork.Product.Update(productVM.Product);
                }
                
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

       /* public IActionResult Delete(int? id)
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
        }*/


        #region APICALLS
        [HttpGet]
        public IActionResult GetAll() 
        {
            List<Product> objProductList = _UnitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new {data=objProductList});
        }
        [HttpDelete]
        public IActionResult Delete(int? id)
        {

            Product? productToBeDeleted = _UnitOfWork.Product.Get(x => x.Id == id);
            if(productToBeDeleted == null)
            {
                return Json(new {success=false,message="Error while deleting"});
            }

            var oldImagePath = Path.Combine(_WebHostEnvironment.WebRootPath, productToBeDeleted.ImageURL.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }
            _UnitOfWork.Product.Remove(productToBeDeleted);
            _UnitOfWork.Save();
            TempData["Success"] = "The Product has been deleted successfully";
            return Json(new { success = true, message = "Deleted successfully" }); ;

        }
        #endregion
    }
}
