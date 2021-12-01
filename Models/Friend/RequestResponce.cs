using System;

namespace TaskPlusPlus.API.Models.Friend
{
    public sealed class RequestResponce : FriendModel
    {
        public Guid Id { get; set; }
        public bool Responce { get; set; }
    }
}
