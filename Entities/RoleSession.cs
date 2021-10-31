using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskPlusPlus.API.Entities
{
    public class RoleSession
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid RoleId { get; set; }

        [Required]
        public Guid BoardId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public bool Demoted { get; set; }

        [Required]
        public DateTime AsignDate { get; set; }

        [Required]
        public DateTime DemotedDate { get; set; }
    }
}
