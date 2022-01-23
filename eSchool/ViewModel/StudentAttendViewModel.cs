using eSchool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eSchool.ViewModel
{
    public class StudentAttendViewModel
    {
        public Student Student { get; set; }
        public string Status { get; set; }
        public int TotalAttendances { get; set; }
        public int TotalAbsences { get; set; }
    }
}
