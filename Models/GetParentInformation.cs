using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskPlusPlus.API.Models
{
    public class GetParentInformation
    {
        public string AccessToken { get; set; }
        public Guid ParentId { get; set; }
    }
}
