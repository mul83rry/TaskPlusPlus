using System;

namespace TaskPlusPlus.API.Models.Employee
{
    public sealed class GetEmployee : EmployeeModel
    {
        public Guid BoardId { get; set; }
    }
}
