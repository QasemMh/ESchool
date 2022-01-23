using eSchool.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eSchool.ViewModel
{
    public class QuizResultViewModel
    {


        public string StudentName { get; set; }
        public int StudentId { get; set; }
        public int Result { get; set; }
        public static int MaxMark { get; set; }

    }
}
