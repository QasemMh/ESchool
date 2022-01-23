using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eSchool.ViewModel
{
    public class GradeViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }

        public double FirstMark { get; set; }
        public double MidtMark { get; set; }
        public double FinalMark { get; set; }
        public double ActivityMark { get; set; }

        public double Total { get; set; }

        public int GradeId { get; set; }
    }
}
