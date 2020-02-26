using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DiziSearch.Models.ViewModels
{
    public class CreateModel
    {
        [Required]
        public string Name { get; set; }
        
        [Required]
        [EmailAddress]
        [Display(Name ="E-posta")]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Admin")]//ADDED
        public string Admin { get; set; }
        public enum EAdmin { Master = 0, Normal = 1, Moderator = 2 }

    }

    public class LoginModel
    {
        [Required]
        [EmailAddress]
        [Display(Name ="E-posta")]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public string ReturnUrl { get; set; } = "/";
    }
}
