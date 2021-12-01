using System;

namespace TaskPlusPlus.API.Models.Comment
{
    public sealed class DeleteComment : CommentModel
    {
        public Guid Id { get; set; }
        public Guid ParentId { get; set; }
    }
}
