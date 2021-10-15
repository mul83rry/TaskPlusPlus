using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskPlusPlus.API.Entities
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } // todo: move to profile table

        [MaxLength(50)]
        public string LastName { get; set; } // todo: move to profile table

        [Required]
        public DateTime SignupDate { get; set; }          

        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } // todo: move to login table
    }
}
