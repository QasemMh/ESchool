using eSchool.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eSchool.ViewModel
{
    public class UserProfileViewModel

    {
        public int UserId { get; set; }
        public string Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Mid Name")]
        public string MidName { get; set; }

        [Required, StringLength(10, ErrorMessage = "The {0} must be {2} characters long.", MinimumLength = 10)]
        public string NationalId { get; set; }

        [Required]
        public char Gender { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [EmailAddress]
        [Display(Name = "Email (optional)")]
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Phone (optional)")]
        public string Phone { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateBirth { get; set; }


        //[Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }



        public string ClassName { get; set; }
        public Address Address { get; set; }
        public int? AddressId { get; set; }
        public Class Class { get; set; }
        [Required]
        [Display(Name = "Class Name")]
        public int ClassId { get; set; }

        public int? ParentId { get; set; }
        public Parent Parent { get; set; }

        public string ImagePath { get; set; }
        public IFormFile Image { get; set; }


    }
}
