using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskPlusPlus.API.Models.Comment
{
    public sealed class GetComment : CommentModel
    {
        public Guid ParentId { get; set; }
    }
}
