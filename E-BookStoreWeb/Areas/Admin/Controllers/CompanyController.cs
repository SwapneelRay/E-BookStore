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
    public class CompanyController : Controller
    {
        public readonly IUnitOfWork _UnitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _UnitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<Company> objCompanyList = _UnitOfWork.Company.GetAll().ToList();
            return View(objCompanyList);
        }

        public IActionResult Upsert(int? id) // update and insert in one
        {
            Company companyObj = new Company();
            if (id == null || id == 0)
            {
                //create
                return View(companyObj);
            }
            else
            {
                //update
                companyObj = _UnitOfWork.Company.Get(u => u.Id == id);
                return View(companyObj);
            }

        }

        [HttpPost]
        public IActionResult Upsert(Company companyObj)
        {
           

            if (ModelState.IsValid)
            {

               
                if (companyObj.Id == 0)
                {
                    _UnitOfWork.Company.Add(companyObj);
                }
                else
                {
                    _UnitOfWork.Company.Update(companyObj);
                }

                _UnitOfWork.Save();
                TempData["Success"] = "New Company has been added successfully";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Failed"] = "New Company Can not be added successfully";
                return View(companyObj);
            }
        }


        #region APICALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> objcompanyList = _UnitOfWork.Company.GetAll().ToList();
            return Json(new { data = objcompanyList });
        }
        [HttpDelete]
        public IActionResult Delete(int? id)
        {

            Company? companyToBeDeleted = _UnitOfWork.Company.Get(x => x.Id == id);
            if (companyToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            _UnitOfWork.Company.Remove(companyToBeDeleted);
            _UnitOfWork.Save();
            TempData["Success"] = "The Company has been deleted successfully";
            return Json(new { success = true, message = "Deleted successfully" }); ;

        }
        #endregion
    }
}
