using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TumpleTestTGBot
{
    class UserState
    {
        public long UserId { get; set; }
        public bool Photo { get; set; }
        public string FullName { get; set; }
        public string GroupNumber { get; set; }
        public string Reason { get; set; }
        public string Date { get; set; }
        public int CurrentStep { get; set; }
    }
}
