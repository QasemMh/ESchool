using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eSchool.ViewModel
{
    public class MessageStudentViewModel
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string CourseName { get; set; }
        public string FromId { get; set; }
        public int ClassId { get; set; }
    }
}
