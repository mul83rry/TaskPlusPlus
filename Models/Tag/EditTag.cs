using System;

namespace TaskPlusPlus.API.Models.Tag
{
    public sealed class EditTag : TagModel
    {
        public Guid Id { get; set; }
        public Guid BoardId { get; set; }
        public string Color { get; set; }
    }
}
