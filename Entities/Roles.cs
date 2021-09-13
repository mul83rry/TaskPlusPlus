using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskPlusPlus.API.Entities
{
    public class Roles
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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
