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

        [Required]
        public string Caption { get; set; }

        [Required]
        public string BackgroundColor { get; set; }

        [Required]
        public Guid BoardId { get; set; }

        [Required]
        public bool TaskRead { get; set; }

        [Required]
        public bool TaskWrite { get; set; }
        
        [Required]
        public bool TaskCompelete { get; set; }

        [Required]
        public bool CommentRead { get; set; }

        [Required]
        public bool CommentWrite { get; set; }

        [Required]
        public bool Deleted { get; set; } 

        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
