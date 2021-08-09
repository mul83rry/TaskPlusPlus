using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskPlusPlus.API.Entities
{
    public class FriendList
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id {get; set;}

        [Required]
        public Guid From { get; set; }


        [Required] 
        public Guid To { get; set; }

        [Required]
        public bool Pending { get; set; }


        [Required]
        public bool Accepted { get; set; }


        [Required]
        public bool Removed { get; set; }

        [Required]
        public DateTime RequestDate { get; set; }
    }
}
