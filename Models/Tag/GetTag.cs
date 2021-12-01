using System;

namespace TaskPlusPlus.API.Models.Tag
{
    public sealed class GetTag : TagModel
    {
        public Guid BoardId { get; set; }
    }
}
