using eSchool.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eSchool.ViewModel
{
    public class TeacherViewModel
    {
        public string AccountId { get; set; }
        public int Id { get; set; }
        [Required, MaxLength(10), MinLength(10)]
        public string NationalId { get; set; }

        [Required, MaxLength(50)]
        public string FirstName { get; set; }
        [MaxLength(50)]
        public string MidName { get; set; }
        [Required, MaxLength(50)]
        public string LastName { get; set; }

        [Required]
        public char Gender { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateBirth { get; set; }

        [MaxLength(250)]
        public string ImagePath { get; set; }



        [EmailAddress]
        public string Email { get; set; }
        public string Phone { get; set; }

        [Required]
        public string UserName { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }


        public int SubjectCount { get; set; }
        public int ClassCount { get; set; }
        public List<Subject> Subjects { get; set; }
        public IEnumerable<EnrollmentViewModel> Enrollments { get; set; }

    }
}
