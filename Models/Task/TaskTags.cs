using System;

namespace TaskPlusPlus.API.Models.Task
{
    public sealed class TaskTags
    {
        public Guid TagId { get; set; }
        public Guid TagListId { get; set; }
        public string Caption { get; set; }
        public string Color { get; set; }
    }
}
