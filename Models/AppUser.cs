using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DiziSearch.Models
{
    public class AppUser : IdentityUser
    {
        [NotMapped]
        [Required]
        [Display(Name = "Admin")]//ADDED
        public string Admin { get; set; }
        public enum EAdmin { Master = 0, Normal = 1, Moderator = 2 }

        [Display(Name = "Currently")]
        public string StatusRole { get; set; }
    }
}
