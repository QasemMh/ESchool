using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eSchool.Models
{
    public class QuizResult
    {
        public int Id { get; set; }
        public int Result { get; set; }




        public int QuizId { get; set; }
        public Quiz Quiz { get; set; }
        public int SubjectId { get; set; }
        public Subject Subject { get; set; }
        public int StudentId { get; set; }
        public Student Student { get; set; }


    }
}

