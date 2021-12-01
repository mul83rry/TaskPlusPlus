using System;

namespace TaskPlusPlus.API.Models.Tag
{
    public sealed class AsignTag : TagModel
    {
        public Guid Id { set; get; }
        public Guid TaskId { get; set; }
    }
}
