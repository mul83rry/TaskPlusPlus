using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskPlusPlus.API.Entities
{
    public class Comment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Text { get; set; }

        [Required]
        public Guid Sender { get; set; }

        [Required]
        public Guid ReplyTo { get; set; }

        [Required]
        public Guid LastModifiedBy { get; set; }
        // todo: admin = if admin edit user comment type sonsored by

        [Required]
        public DateTime CreationDate { get; set; }

        [Required]
        public string EditId { get; set; }
        // todo: check for default guid

        [Required]
        public bool Deleted { get; set; }

    }
}
