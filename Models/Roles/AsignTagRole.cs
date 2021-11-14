using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskPlusPlus.API.Models.Roles
{
    public sealed class AsignTagRole : RoleModel
    {
        public Guid BoardId { get; set; }
        public Guid RoleId { get; set; }
        public Guid TagId { get; set; }
    }
}
