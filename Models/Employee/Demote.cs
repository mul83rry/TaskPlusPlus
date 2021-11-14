using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskPlusPlus.API.Models.Employee
{
    public sealed class Demote : EmployeeModel
    {
        public Guid BoardId { get; set; }
        public Guid RoleSessionId { get; set; }
    }
}
