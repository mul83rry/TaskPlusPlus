using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskPlusPlus.API.Models.Tag
{
    public sealed class AsignTag : TagModel
    {
        public Guid Id { set; get; }
        public Guid TaskId { get; set; }
    }
}
