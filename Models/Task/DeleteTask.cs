using System;

namespace TaskPlusPlus.API.Models.Task
{
    public sealed class DeleteTask : TaskModel
    {
        public Guid Id { get; set; }
    }
}
