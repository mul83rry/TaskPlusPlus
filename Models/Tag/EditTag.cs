using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskPlusPlus.API.Models.Tag
{
    public sealed class EditTag : TagModel
    {
        public Guid Id { get; set; }
        public Guid BoardId { get; set; }
        public string Color { get; set; }
    }
}
