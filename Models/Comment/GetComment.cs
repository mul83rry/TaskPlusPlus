using System;

namespace TaskPlusPlus.API.Models.Comment
{
    public sealed class GetComment : CommentModel
    {
        public Guid ParentId { get; set; }
    }
}
