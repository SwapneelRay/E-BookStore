using E_BookStore.DataAccess.Repository;
using E_BookStore.DataAccess.Repository.IRepository;
using E_BookStore.Models;
using E_BookStore.Models.ViewModels;
using E_BookStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace E_BookStoreWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = (SD.Role_Admin))]
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
                productVM.Product = _UnitOfWork.Product.Get(u=>u.Id==id,includeProperties: "productImages");
                return View(productVM);
            }
            
        }
        public IActionResult DeleteImage(int id) 
        {
            var imageToBeDeleted = _UnitOfWork.ProductImage.Get(u => u.Id == id);
            var productId = imageToBeDeleted.ProductId;
            if (imageToBeDeleted != null)
            {
                if (!string.IsNullOrEmpty(imageToBeDeleted.ImageURL))
                {
                    var path = Path.Combine(_WebHostEnvironment.WebRootPath, imageToBeDeleted.ImageURL.TrimStart('\\'));
                    if (System.IO.File.Exists(path)) { 
                        System.IO.File.Delete(path);
                    }
                }
                _UnitOfWork.ProductImage.Remove(imageToBeDeleted);
                _UnitOfWork.Save();
                TempData["success"] = "Image Deleted";
            }
            return RedirectToAction(nameof(Upsert), new {id=productId});
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, List<IFormFile?> files)
        {
          
            if (ModelState.IsValid)
            {
                if (productVM.Product.Id == 0)
                {
                    _UnitOfWork.Product.Add(productVM.Product);
                }
                else
                {
                    _UnitOfWork.Product.Update(productVM.Product);
                }

                _UnitOfWork.Save();

                string wwwRootPath = _WebHostEnvironment.WebRootPath;
                if(files != null)
                {
                    foreach (var file in files) { 
                    
                        string filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string productPath = Path.Combine(wwwRootPath,$@"images\product\{productVM.Product.Id}");
                        
                        if (!Directory.Exists(productPath))
                        {
                            Directory.CreateDirectory(productPath);
                        }
                        
                        using (var fileStream = new FileStream(Path.Combine(productPath, filename), FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }
                        ProductImage productImage = new() { 
                            ImageURL = @"\"+ $@"images\product\{productVM.Product.Id}" + @"\"+filename,
                            ProductId = productVM.Product.Id,
                        };

                        if (productVM.Product.productImages == null)
                            productVM.Product.productImages = new List<ProductImage>();

                        productVM.Product.productImages.Add(productImage);
                    }
                    _UnitOfWork.Product.Update(productVM.Product);
                    _UnitOfWork.Save();
                }
                
                
               
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
            

           

            if (productToBeDeleted == null)
            {
                return Json(new {success=false,message="Error while deleting"});
            }
            var imagepath = Path.Combine(_WebHostEnvironment.WebRootPath, $@"images\product\{productToBeDeleted.Id}");

            if (Directory.Exists(imagepath))
            {
                Directory.Delete(imagepath, true);
            }

            IEnumerable<ProductImage> images = _UnitOfWork.ProductImage.GetAll(x => x.ProductId == id);
            _UnitOfWork.ProductImage.RemoveRange(images);
            _UnitOfWork.Product.Remove(productToBeDeleted);
            _UnitOfWork.Save();
            TempData["Success"] = "The Product has been deleted successfully";
            return Json(new { success = true, message = "Deleted successfully" }); ;

        }
        #endregion
    }
}
