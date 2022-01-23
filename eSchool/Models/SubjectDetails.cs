using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eSchool.Models
{
    public class SubjectDetails
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; }
        [MaxLength(250)]
        public string Description { get; set; }

        public List<Subject> Subjects { get; set; }
    }
}
