using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskPlusPlus.API.Entities
{
    public class Message
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public DateTime CreationAt { get; set; }

        [Required]
        public bool Seen { get; set; }

        [Required]
        public bool Deleted { get; set; }
    }
}
