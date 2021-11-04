using System;

namespace TaskPlusPlus.API.Models.Task
{
    public sealed class AddTask : TaskModel
    {
        public Guid ParentId { get; set; }
        public string Caption { get; set; }
    }
}
