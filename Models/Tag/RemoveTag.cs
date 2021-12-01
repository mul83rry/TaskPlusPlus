using System;

namespace TaskPlusPlus.API.Models.Tag
{
    public sealed class RemoveTag : TagModel
    {
        public Guid Id { get; set; }
        public Guid BoardId { get; set; }
    }
}
