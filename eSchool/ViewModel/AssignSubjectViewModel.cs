using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eSchool.ViewModel
{
    public class AssignSubjectViewModel
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Subject")]
        public int SubjectDetailsId { get; set; }
        [Required]
        [Display(Name = "Class")]
        public int ClassId { get; set; }
        [Required]
        [Display(Name = "Teacher")]
        public int TeacherId { get; set; }


        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }


    }
}
