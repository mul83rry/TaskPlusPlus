using System;

namespace TaskPlusPlus.API.Models.Tag
{
    public sealed class AddTag : TagModel
    {
        public Guid BoardId { get; set; }
        public string Caption { get; set; }
    }
}
