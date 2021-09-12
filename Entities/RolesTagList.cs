using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace TaskPlusPlus.API.Entities
{
    public class RolesTagList
    {
        [Key]
        public Guid Id { get; set; }
        public Guid RoleId { get; set; }
        public Guid TagId { get; set; }

    }
}
