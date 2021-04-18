using System;
using System.ComponentModel.DataAnnotations;

namespace TaskPlusPlus.API.Entities
{
    public class AssignTo
    {
        [Key]
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public Guid UserId { get; set; }
    }
}
