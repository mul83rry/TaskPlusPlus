using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskPlusPlus.API.Models.Comment
{
    public sealed class EditComment : CommentModel
    {
        public Guid Id { get; set; }
        public Guid ParentId { get; set; }
        public string Content { get; set; }
    }
}
