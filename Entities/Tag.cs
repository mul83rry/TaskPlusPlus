using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TaskPlusPlus.API.Entities
{
    public class Tag
    {
        [Key]
        public Guid Id { get; set; }
        public Guid BoardId { get; set; }
        public string Caption { get; set; }
        public bool Removed { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
