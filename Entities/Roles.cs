using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace TaskPlusPlus.API.Entities
{
    public class Roles
    {
        [Key]
        public Guid Id { get; set; }
        public string Caption { get; set; }
        public Guid BoardId { get; set; }
        public bool TaskRead { get; set; }
        public bool TaskWrite { get; set; }
        public bool CommentRead { get; set; }
        public bool CommentWrite { get; set; }
        public bool Removed { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
