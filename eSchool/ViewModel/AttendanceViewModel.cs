using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eSchool.ViewModel
{
    public class AttendanceViewModel
    {
        public static int SubjectId { get; set; }
        public static int ClassId { get; set; }



        public List<CheckboxViewModel> AttendencesList { get; set; }

    }
}
