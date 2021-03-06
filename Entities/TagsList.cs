using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskPlusPlus.API.Entities
{
    public class TagsList
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid TagId { get; set; }

        [Required]
        public Guid TaskId { get; set; }
        
        [Required]
        public bool Deleted { get; set; } 

        [Required]
        public DateTime AsignDate { get; set; }
    }
}
