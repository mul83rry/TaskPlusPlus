using System;

namespace TaskPlusPlus.API.Models.Task
{
    public sealed class Task : TaskModel
    {
        public Guid ParentId { get; set; }
    }
}
