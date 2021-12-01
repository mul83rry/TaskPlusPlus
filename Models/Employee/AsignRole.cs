using System;

namespace TaskPlusPlus.API.Models.Employee
{
    public sealed class AsignRole : EmployeeModel
    {
        public Guid EmployeeId { get; set; }
        public Guid RoleId { get; set; }
        public Guid BoardId { get; set; }
    }
}
