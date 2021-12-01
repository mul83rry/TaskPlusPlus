using System;

namespace TaskPlusPlus.API.Models.Board
{
    public sealed class EditBoard : BoardModel
    {
        public Guid Id { get; set; }
        public string Caption { get; set; }
    }
}
