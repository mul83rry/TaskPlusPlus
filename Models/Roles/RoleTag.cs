using System;

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
