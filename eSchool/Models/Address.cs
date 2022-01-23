using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eSchool.Models
{
    public class Address
    {
        public int Id { get; set; }
        [Required, MaxLength(250)]
        public string Address1 { get; set; }
        [MaxLength(250)]
        [Display(Name = "Address2(optional)")]
        public string Address2 { get; set; }
        [MaxLength(50)]
        [Display(Name = "District(optional)")]
        public string District { get; set; }
        [MaxLength(50)]
        [Display(Name = "Location(optional)")]
        public string Location { get; set; }


        public Student Student { get; set; }

    }
}
