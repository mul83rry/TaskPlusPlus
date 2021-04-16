using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseLibrary.API.Entities
{
    public class SharedBoard
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid BoardId { get; set; }

        [Required]
        public Guid ShareTo { get; set; }

        [Required]
        public DateTime GrantedAccessAt { get; set; }
    }
}
