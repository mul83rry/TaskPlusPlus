using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskPlusPlus.API.Models.Employee
{
    public class EmployeeRoles
    {
        public Guid RoleId { get; set; }
        public Guid RoleSessionId { get; set; }
        public string Caption { get; set; }
        public string Color { get; set; }
    }
}
