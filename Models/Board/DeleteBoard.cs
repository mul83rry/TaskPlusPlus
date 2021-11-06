using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskPlusPlus.API.Models.Board
{
    public sealed class DeleteBoard : BoardModel
    {
        public Guid Id { get; set; }
    }
}
