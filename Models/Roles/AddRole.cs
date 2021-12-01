using System;

namespace TaskPlusPlus.API.Models.Roles
{
    public sealed class AddRole : RoleModel
    {
        public Guid BoardId { get; set; }
        public string Caption { get; set; }
        public string Color { get; set; }
        public bool ReadTask { get; set; }
        public bool WriteTask { get; set; }
        public bool CompleteTask { get; set; }
        public bool ReadComment { get; set; } 
        public bool WriteComment { get; set; }
    }
}
