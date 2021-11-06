using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskPlusPlus.API.Models.Board
{
    public sealed class ShareBoard : BoardModel
    {
        public Guid BoardId { get; set; }
        public Guid[] ShareToList { get; set; }
    }
}
