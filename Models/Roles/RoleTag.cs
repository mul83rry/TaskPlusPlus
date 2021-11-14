using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskPlusPlus.API.Models.Roles
{
    public class RoleTag
    {
       public Guid RoleTagId { get; set; }
       public Guid TagId { get; set; }
       public string Caption { get; set; }
       public string Color { get; set; }
    }
}
