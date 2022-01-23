using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eSchool.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int? TeacherId { get; set; }
        public Teacher Teacher { get; set; }
        public int? ParentId { get; set; }
        public Parent Parent { get; set; }
        public int? StudentId { get; set; }
        public Student Student { get; set; }


    }
}
