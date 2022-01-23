using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace eSchool.Models
{
    public class Chat
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Title { get; set; }
        [Required]
        [MaxLength(500)]
        public string Message { get; set; }

        [Required]
        public string FromId { get; set; }
        [Required]
        public string ToId { get; set; }


        public DateTime SendDate { get; set; }

        public ApplicationUser From { get; set; }
        public ApplicationUser To { get; set; }
    }
}
