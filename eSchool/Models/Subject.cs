using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eSchool.Models
{
    public class Subject
    {
        public int Id { get; set; }
        [Required]
        public int SubjectDetailsId { get; set; }
        public SubjectDetails SubjectDetails { get; set; }
        [Required]
        public int ClassId { get; set; }
        public Class Class { get; set; }
        [Required]
        public int TeacherId { get; set; }
        public Teacher Teacher { get; set; }
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        public List<Grade> Grades { get; set; }

        public List<Lesson> Lessons { get; set; }

        public List<QuizResult> QuizResults { get; set; }
        public List<Quiz> Quizzes { get; set; }
    }
}
