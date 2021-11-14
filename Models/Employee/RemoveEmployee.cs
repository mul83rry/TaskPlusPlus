using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskPlusPlus.API.Models.Employee
{
    public sealed class RemoveEmployee : EmployeeModel
    {
        public Guid BoardId { get; set; }
        public Guid ShareId { get; set; }
    }
}
