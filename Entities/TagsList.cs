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
        public Guid TagId { get; set; }
        public Guid TaskId { get; set; }
        public bool Removed { get; set; }
        public DateTime AsignDate { get; set; }
    }
}
