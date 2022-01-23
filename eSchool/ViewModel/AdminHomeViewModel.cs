using eSchool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eSchool.ViewModel
{
    public class AdminHomeViewModel
    {
        public List<Notice> Notice { get; set; }
        public List<Chat> Chats { get; set; }
    }
}
