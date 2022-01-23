using eSchool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eSchool.ViewModel
{
    public class ClassDataViewModel
    {
        public Class Class { get; set; }
        public List<Subject> Subjects { get; set; }
        public List<Subject> SubjectsNames { get; set; }
    }
}
