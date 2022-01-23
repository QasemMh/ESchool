using eSchool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eSchool.ViewModel
{
    public class ViewExamResultViewModel
    {
        public List<Student> Students { get; set; }
        public List<QuizResult> QuizResults { get; set; }
        public Subject Subject { get; set; }

        public Quiz ExamData { get; set; }

        public double Avarage { get; set; }
    }
}
