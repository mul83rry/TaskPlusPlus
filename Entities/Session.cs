using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskPlusPlus.API.Entities
{
    public class Session
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string AccessToken { get; set; }

        [Required]
        public bool IsValid { get; set; }

        [Required]
        public DateTime CreationAt { get; set; }

        [Required]
        public DateTime LastFetchTime { get; set; }
    }
}
