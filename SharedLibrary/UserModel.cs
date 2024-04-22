using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary
{
    public class UserModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; }

        public byte[] Salt { get; set; }
        public string HashedPassword { get; set; }

        public string Password { get; set; }
    }
}
