using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskPlusPlus.API.Models.Employee
{
    public sealed class GetEmployee : EmployeeModel
    {
        public Guid BoardId { get; set; }
    }
}
