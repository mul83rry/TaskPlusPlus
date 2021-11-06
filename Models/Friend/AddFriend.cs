using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskPlusPlus.API.Models.Friend
{
    public sealed class AddFriend : FriendModel
    {
        public string PhoneNumber { get; set; }
    }
}
