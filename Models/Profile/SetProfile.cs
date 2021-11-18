using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskPlusPlus.API.Models.Profile
{
    public sealed class SetProfile : ProfileModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Bio { get; set; }
        public string Img { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}
