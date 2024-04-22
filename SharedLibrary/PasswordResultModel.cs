using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary
{
    public class PasswordResultModel
    {
            public string HashPassword { get; set; }
            public byte[] Salt { get; set; }
        
    }
}
