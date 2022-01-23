using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eSchool.Models
{
    public class Grade
    {
        public int Id { get; set; }
        public double? FirstMark { get; set; }
        public double? MidtMark { get; set; }
        public double? FinalMark { get; set; }
        public double? ActivityMark { get; set; }
        public double? Total { get; set; }



        public int SubjectId { get; set; }
        public Subject Subject { get; set; }


        public int StudentId { get; set; }
        public Student Student { get; set; }
    }
}
