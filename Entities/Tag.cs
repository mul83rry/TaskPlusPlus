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
        public Guid BoardId { get; set; }
        public string Caption { get; set; }
        public bool Removed { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
