using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskPlusPlus.API.Model
{
    public class SignIn
    {
        public string phoneNumber { get; set; }
        public string osVersion { get; set; }
        public string deviceType { get; set; }
        public string browerVersion { get; set; }
        public string orientation { get; set; }
    }
}
