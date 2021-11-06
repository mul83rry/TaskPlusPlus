using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskPlusPlus.API.Models.Friend
{
    public sealed class RequestResponce : FriendModel
    {
        public Guid Id { get; set; }
        public bool Responce { get; set; }
    }
}
