using System;

namespace TaskPlusPlus.API.Models.Employee
{
    public sealed class Demote : EmployeeModel
    {
        public Guid BoardId { get; set; }
        public Guid RoleSessionId { get; set; }
    }
}
