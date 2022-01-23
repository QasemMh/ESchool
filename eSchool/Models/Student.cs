using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eSchool.Models
{
    public class Student
    {
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

        public ApplicationUser ApplicationUser { get; set; }

        public int? ParentId { get; set; }
        public Parent Parent { get; set; }
        public int? AddressId { get; set; }
        public Address Address { get; set; }
        public int? ClassId { get; set; }
        public Class Class { get; set; }


        public List<QuizResult> QuizResults { get; set; }
        public List<Grade> Grades { get; set; }
        public List<Attendance> Attendances { get; set; }
        public List<Absence> Absences { get; set; }
    }
}
