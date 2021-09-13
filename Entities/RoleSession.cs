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
        public Guid RoleId { get; set; }
        public Guid BoardId { get; set; }
        public Guid UserId { get; set; }
        public bool Demoted { get; set; }
        public DateTime AsignDate { get; set; }
    }
}
