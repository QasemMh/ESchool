using eSchool.Models;
using System.Collections.Generic;

namespace eSchool.ViewModel
{
    public class SearchPageViewModel
    {
        public List<Student> Students { get; set; }
        public List<Teacher> Teachers { get; set; }
        public List<Parent> Parents { get; set; }
        public List<Subject> Classes { get; set; }

    }
}