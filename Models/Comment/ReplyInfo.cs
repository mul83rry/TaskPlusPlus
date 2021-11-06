using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskPlusPlus.API.Models.Comment
{
    public sealed class ReplyInfo
    {
        public string Sender { get; set; }
        public string Content { get; set; }
    }
}
