using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskPlusPlus.API.Model
{
    public class SignIn
    {
        public string PhoneNumber { get; set; }
        public string OsVersion { get; set; }
        public string DeviceType { get; set; }
        public string BrowerVersion { get; set; }
        public string Orientation { get; set; }
    }
}
