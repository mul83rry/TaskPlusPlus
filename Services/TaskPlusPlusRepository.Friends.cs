using TaskPlusPlus.API.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaskPlusPlus.API.DbContexts;

namespace TaskPlusPlus.API.Services
{
    public partial class TaskPlusPlusRepository
    {
        public async Task<JObject> AddFriendAsync(string accessToken, string phoneNumber)
        {
            using var context = new TaskPlusPlusContext();
            var user = await GetUserSessionAsync(accessToken, context);
            var friendUser = await context.Profiles.SingleOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
            if (friendUser == null) return JsonMap.FalseResult;
            if (friendUser.UserId == user.UserId) return JsonMap.FalseResult;

            // if already there is an active request between these two return false
            if (await context.FriendLists.AnyAsync(f =>
            (f.From == user.UserId && f.To == friendUser.UserId && (!f.Removed && f.Accepted || f.Pending)) ||
            (f.To == user.UserId && f.From == friendUser.UserId && (!f.Removed && f.Accepted || f.Pending))))
                return JsonMap.FalseResult;

            var friend = new FriendList()
            {
                Id = Guid.NewGuid(),
                From = user.UserId,
                To = friendUser.UserId,
                Pending = true,
                Accepted = false,
                RequestDate = DateTime.Now,
                Removed = false,
                ApplyDate = DateTime.Now,
            };

            await context.FriendLists.AddAsync(friend);
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<string> GetFriendsListAsync(string accessToken)
        {
            using var context = new TaskPlusPlusContext();
            var user = await GetUserSessionAsync(accessToken, context);

            var res = from friendsList in context.FriendLists.Where(f => (user.UserId == f.From || user.UserId == f.To) && f.Accepted && !f.Removed).OrderBy(f => f.RequestDate)
                      select new
                      {
                          friendsList.From,
                          friendsList.To,
                          friendsList.Id
                      };

            var jsonData = new JArray();

            foreach (var item in res)
            {
                Profile userDetail = null;

                if (item.From == user.UserId) userDetail = await GetUser(item.To, context);
                else if (item.To == user.UserId) userDetail = await GetUser(item.From, context);

                jsonData.Add(new JObject
                    {
                        {"Id", item.Id},
                        {"FirstName", userDetail.FirstName },
                        {"LastName", userDetail.LastName},
                        {"PhoneNumber", userDetail.PhoneNumber},
                        {"FriendId", userDetail.UserId},
                        {"Bio", userDetail.Bio},
                    });
            }

            return jsonData.ToString();
        }

        public async Task<string> GetFriendRequestQueueAsync(string accessToken)
        {
            using var context = new TaskPlusPlusContext();
            var user = await GetUserSessionAsync(accessToken, context);
            var res = from FList in context.FriendLists.Where(f => user.UserId == f.To && f.Pending).OrderBy(f => f.RequestDate)
                      select new
                      {
                          FList.From,
                          FList.Id
                      };

            var jsonData = new JArray();

            foreach (var item in res)
            {
                var userDetail = await GetUser(item.From, context);
                jsonData.Add(new JObject
                    {
                        {"Id", item.Id},
                        {"FirstName", userDetail.FirstName },
                        {"LastName", userDetail.LastName},
                        {"Bio", userDetail.Bio}
                    });
            }

            return jsonData.ToString();
        }

        public async Task<JObject> ApplyFriendRequestResponceAsync(string accessToken, Guid requestId, bool reply)
        {
            using var context = new TaskPlusPlusContext();
            var user = await GetUserSessionAsync(accessToken, context);
            var request = await context.FriendLists.SingleOrDefaultAsync(f => f.Id == requestId);
            if (user.UserId != request.To) return JsonMap.FalseResult;
            if (!request.Pending) return JsonMap.FalseResult;

            request.Pending = false;
            request.Accepted = reply;
            request.ApplyDate = DateTime.Now;

            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<JObject> RemoveFriendAsync(string accessToken, Guid requestId)
        {
            using var context = new TaskPlusPlusContext();
            var user = await GetUserSessionAsync(accessToken, context);
            var request = await context.FriendLists.SingleAsync(f => f.Id == requestId);
            if (request.From != user.UserId && request.To != user.UserId) return JsonMap.FalseResult;

            request.Removed = true;
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }
    }
}
