using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary
{
    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; } // skal ændres til hashed passwords siden man ikke må gemme det direkte.
    }
}
