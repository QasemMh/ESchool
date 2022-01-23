using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eSchool.Models
{
    public class Lesson
    {
        public int Id { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public int SubjectId { get; set; }
        public Subject Subject { get; set; }

        public List<Attendance> Attendances { get; set; }
        public List<Absence> Absences { get; set; }
    }
}
