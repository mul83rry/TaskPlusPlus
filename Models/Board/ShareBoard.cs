using System;

namespace TaskPlusPlus.API.Models.Board
{
    public sealed class ShareBoard : BoardModel
    {
        public Guid BoardId { get; set; }
        public Guid[] ShareToList { get; set; }
    }
}
