using eSchool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eSchool.ViewModel
{
    public class ViewAttendanceViewModel
    {

        public List<StudentAttendViewModel> StudentsAttends { get; set; }
        public string SubjectTime { get; set; }
        public string ClassName { get; set; }
        public string SubjectName { get; set; }
        public string LessonDate { get; set; }
        public bool NoLessons { get; set; }



    }
}
