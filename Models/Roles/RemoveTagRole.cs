using System;

namespace TaskPlusPlus.API.Models.Roles
{
    public sealed class RemoveTagRole : RoleModel
    {
        public Guid BoardId { get; set; }
        public Guid RoleTagId { get; set; }
    }
}
