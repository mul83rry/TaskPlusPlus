using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskPlusPlus.API.Models.Task
{
    public sealed class TaskTags
    {
        public Guid Id { get; set; }
        public string Caption { get; set; }
        public string Color { get; set; }
    }
}
