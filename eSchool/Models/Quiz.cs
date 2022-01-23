using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eSchool.Models
{
    public class Quiz
    {
        public int Id { get; set; }
        [Required, MaxLength(50)]
        public string Name { get; set; }
        [Required]
        public int Mark { get; set; }
        [Required]
        public DateTime Date { get; set; }

        public List<QuizResult> QuizResults { get; set; }

        public int? SubjectId { get; set; }
        public Subject Subject { get; set; }
    }
}
