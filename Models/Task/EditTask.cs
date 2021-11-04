using System;

namespace TaskPlusPlus.API.Models.Task
{
    public sealed class EditTask : TaskModel
    {
        public Guid Id { get; set; }
        public string Caption { get; set; }
        public bool Star { get; set; }

    }
}
