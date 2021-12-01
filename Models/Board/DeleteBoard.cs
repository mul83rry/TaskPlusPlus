using System;

namespace TaskPlusPlus.API.Models.Board
{
    public sealed class DeleteBoard : BoardModel
    {
        public Guid Id { get; set; }
    }
}
