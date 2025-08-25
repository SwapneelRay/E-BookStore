using E_BookStore.DataAccess.Data;
using E_BookStore.DataAccess.Repository;
using E_BookStore.DataAccess.Repository.IRepository;
using E_BookStore.Models;
using E_BookStore.Models.ViewModels;
using E_BookStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace E_BookStoreWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = (SD.Role_Admin))]
    public class UserController : Controller
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;


        public UserController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _UnitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult RoleManagement(string userid)
        {
            RoleManagementVM roleVM = new RoleManagementVM()
            {
                ApplicationUser = _UnitOfWork.ApplicationUser.Get(u=>u.Id ==userid,includeProperties:"Company"),
                RoleList = _roleManager.Roles.Select(i=> new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Name
                }),
                CompanyList = _UnitOfWork.Company.GetAll().Select(i=> new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                })
            };
            roleVM.ApplicationUser.Role=_userManager.GetRolesAsync(_UnitOfWork.ApplicationUser.Get(u=>u.Id==userid)).GetAwaiter().GetResult().FirstOrDefault();
            return View(roleVM);
        }
               
        #region APICALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            
            List<ApplicationUser> objUserList = _UnitOfWork.ApplicationUser.GetAll(includeProperties:"Company").ToList();


            foreach(var item in objUserList)
            {
                item.Role = _userManager.GetRolesAsync(item).GetAwaiter().GetResult().FirstOrDefault();
                if(item.Company == null)
                {
                    item.Company = new Company()
                    {
                        Name = ""
                    };
                    
                }
            }
            return Json(new { data = objUserList });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody]string id)
        {

            var objFromDb = _UnitOfWork.ApplicationUser.Get(u => u.Id == id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while Locking/Unlocking" });
            }
            
            if(objFromDb.LockoutEnd != null && objFromDb.LockoutEnd > DateTime.Now)
            {
                objFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                objFromDb.LockoutEnd = DateTime.Now.AddYears(1);
            }
            _UnitOfWork.ApplicationUser.Update(objFromDb);
            _UnitOfWork.Save();

            return Json(new { success = true, message = "successfully" }); ;

        }
        [HttpPost]
        public IActionResult RoleManagement(RoleManagementVM roleVM)
        {
            string oldRole = _userManager.GetRolesAsync(_UnitOfWork.ApplicationUser.Get(u => u.Id == roleVM.ApplicationUser.Id)).GetAwaiter().GetResult().FirstOrDefault();
            ApplicationUser user = _UnitOfWork.ApplicationUser.Get(u => u.Id == roleVM.ApplicationUser.Id);
            if (!(roleVM.ApplicationUser.Role == oldRole))
            {
                if (roleVM.ApplicationUser.Role == SD.Role_Company)
                {
                    user.CompanyId = roleVM.ApplicationUser.CompanyId;
                }
                if (oldRole == SD.Role_Company)
                {
                    user.CompanyId = null;
                }
                _UnitOfWork.ApplicationUser.Update(user);
                _UnitOfWork.Save();
                if (oldRole != null) { 
                
                    _userManager.RemoveFromRoleAsync(user, oldRole).GetAwaiter().GetResult();
                }

                _userManager.AddToRoleAsync(user, roleVM.ApplicationUser.Role).GetAwaiter().GetResult();
            }
            else 
            {
                if (oldRole == SD.Role_Company && user.CompanyId != roleVM.ApplicationUser.CompanyId)
                {
                    user.CompanyId = roleVM.ApplicationUser.CompanyId;
                    _UnitOfWork.ApplicationUser.Update(user);
                    _UnitOfWork.Save();
                }

            }
            return RedirectToAction(nameof(Index));
        }
        #endregion
    }
}
