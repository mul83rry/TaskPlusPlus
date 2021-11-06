using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskPlusPlus.API.Models.Comment
{
    public sealed class AddComment : CommentModel
    {
        public string Content { get; set; }
        public Guid ReplyTo { get; set; }
        public Guid ParentId { get; set; }
    }

}
