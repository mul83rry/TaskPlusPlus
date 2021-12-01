using System;

namespace TaskPlusPlus.API.Models.Roles
{
    public sealed class GetRole : RoleModel
    {
        public Guid BoardId { get; set; }
    }
}
