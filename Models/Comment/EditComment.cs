using System;

namespace TaskPlusPlus.API.Models.Comment
{
    public sealed class EditComment : CommentModel
    {
        public Guid Id { get; set; }
        public Guid ParentId { get; set; }
        public string Content { get; set; }
    }
}
