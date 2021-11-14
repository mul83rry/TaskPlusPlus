using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskPlusPlus.API.Models.Roles
{
    public sealed class RemoveTagRole : RoleModel
    {
        public Guid BoardId { get; set; }
        public Guid RoleTagId { get; set; }
    }
}
