using System;

namespace TaskPlusPlus.API.Models.Tag
{
    public sealed class RemoveAssignment : TagModel
    {
        public Guid TaskTagId { get; set; }
        public Guid TaskId { get; set; }
    }
}
