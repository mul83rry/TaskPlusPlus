using System;

namespace TaskPlusPlus.API.Models.Roles
{
    public sealed class RemoveRole : RoleModel
    {
        public Guid BoardId { get; set; }
        public Guid Id { get; set; }
    }
}
