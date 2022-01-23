using eSchool.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eSchool.ViewModel
{
    public class ExamViewModel
    {
        public int CourseId { get; set; }
        public SelectList ExamList { get; set; }
        public Subject Subject { get; set; }
        public int SubjectId { get; set; }
        public int ExamId { get; set; }
    }
}
