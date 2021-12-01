using System;

namespace TaskPlusPlus.API.Models.Comment
{
    public sealed class AddComment : CommentModel
    {
        public string Content { get; set; }
        public Guid ReplyTo { get; set; }
        public Guid ParentId { get; set; }
    }

}
