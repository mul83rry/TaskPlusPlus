using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskPlusPlus.API.Entities
{
    public class Board
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid CreatorId { get; set; }
        
        [Required]
        public string Caption { get; set; }

        [Required]
        public DateTime CreationAt { get; set; }

        [Required]
        public bool Deleted { get; set; }
    }
}
