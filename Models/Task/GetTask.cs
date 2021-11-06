using System;

namespace TaskPlusPlus.API.Models.Task
{
    public sealed class GetTask : TaskModel
    {
        public Guid ParentId { get; set; }
    }
}
