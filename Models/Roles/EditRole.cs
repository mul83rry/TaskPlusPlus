using System;

namespace TaskPlusPlus.API.Models.Roles
{
    public sealed class EditRole : RoleModel
    {
        public Guid Id { get; set; }
        public Guid BoardId { get; set; }
        public string Color { get; set; }
        public bool ReadTask { get; set; }
        public bool WriteTask { get; set; }
        public bool CompleteTask { get; set; }
        public bool ReadComment { get; set; }
        public bool WriteComment { get; set; }
    }
}
