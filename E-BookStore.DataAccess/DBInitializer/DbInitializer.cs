using E_BookStore.DataAccess.Data;
using E_BookStore.Models;
using E_BookStore.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_BookStore.DataAccess.DBInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _db;
            
        public DbInitializer(RoleManager<IdentityRole> roleManager,UserManager<IdentityUser> userManager, ApplicationDbContext db) 
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _db = db;
        }
        public void Initializer()
        {
            try
            {

            // migration if they are not applied
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch(Exception ex)
            {

            }


            // create roles if they are not created
            // if roles are not created, then we will create admin user as well
            if (!_roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();

                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "swapneel.ray@gmail.com",
                    Email = "swapneel.ray@gmail.com",
                    Name = "Swapneel Ray",
                    PhoneNumber = "1112223333",
                    StreetAddress = "test 123 Ave",
                    State = "IL",
                    PostalCode = "23422",
                    City = "Chicago"
                }, "Admin@123").GetAwaiter().GetResult();

                ApplicationUser user = _db.applicationUsers.FirstOrDefault(u=>u.Email== "swapneel.ray@gmail.com");
                _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
            }
            return;
        }
    }
}
