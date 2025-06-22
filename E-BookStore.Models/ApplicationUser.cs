using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_BookStore.Models
{
    public class ApplicationUser:IdentityUser
    {
        [Required]
        public string Name;
        public string? StreetAddress;
        public string? City;
        public string? State;
        public string? PostalCode;

    }
}
