using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskPlusPlus.API.Models.Tag
{
    public sealed class GetTag : TagModel
    {
        public Guid BoardId { get; set; }
    }
}
