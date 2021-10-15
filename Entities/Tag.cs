using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskPlusPlus.API.Entities
{
    public class Tag
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid BoardId { get; set; }

        [Required]
        public string Caption { get; set; }

        [Required]
        public bool Removed { get; set; } // todo: rename to 'Deleted'

        [Required]
        public DateTime CreationDate { get; set; }
    }
}
