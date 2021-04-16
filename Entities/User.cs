using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseLibrary.API.Entities
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [MaxLength(50)]
        public string LastName { get; set; }

        [Required]
        public DateTime SignupDate { get; set; }          

        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        /*public ICollection<Course> Courses { get; set; }
            = new List<Course>();*/
    }
}
