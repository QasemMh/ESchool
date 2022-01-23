using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eSchool.ViewModel
{
    public class AddGradeViewModel
    {
        public List<GradeViewModel> StudentsGrades { get; set; }
        public string ClassName { get; set; }
        public string SubjectName { get; set; }

        public double ClassAvarage { get; set; }
        public double FirstAvarage { get; set; }
        public double MidAvarage { get; set; }
        public double FinalAvarage { get; set; }
    }
}
