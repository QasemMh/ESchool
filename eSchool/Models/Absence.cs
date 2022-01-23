using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eSchool.Models
{
    public class Absence
    {
        public int Id { get; set; }
        [Required, MaxLength(50)]
        public string Resone { get; set; }

        [Required]
        public int StudentId { get; set; }
        public Student Student { get; set; }

        [Required]
        public int LessonId { get; set; }
        public Lesson Lesson { get; set; }
    }
}
