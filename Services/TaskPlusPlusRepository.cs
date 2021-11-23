using TaskPlusPlus.API.DbContexts;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TaskPlusPlus.API.Services
{
    public partial class TaskPlusPlusRepository : ITaskPlusPlusRepository, IDisposable
    {
        private TaskPlusPlusContext context;

        public TaskPlusPlusRepository(TaskPlusPlusContext context) => this.context = context ?? throw new ArgumentNullException(nameof(context));

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || context == null) return;
            context.Dispose();
            context = null;
        }


        private async Task<bool> HasPermissions(Guid userId, Guid parentId, Permissions permissionType)
        {
            var boardId = await GetBoardIdAsync(parentId);
            var RoleAccess = await HasRoleAccess(boardId, userId, permissionType);
            var tagAccess = await HasTagAccess(boardId, userId, parentId);

            if (!RoleAccess || !tagAccess) return false;

            return true;
        }

        public async Task<string> GetRecentChangesAsync(string accessToken)
        {
            var user = await GetUserSessionAsync(accessToken);

            var pendingFriendRequests = from friends in context.FriendLists.Where(f => f.Pending && f.To == user.UserId)
                                        select new 
                                        {
                                            friends.Id,
                                        };

            var newMessages = from msg in context.Messages.Where(m => !m.Seen && m.UserId == user.UserId)
                              select new
                              {
                                  msg.Id,
                              };

            var JsonData = new JObject
            {
                new JProperty("NewMessages",newMessages.Count().ToString()),
                new JProperty("NewFriends", pendingFriendRequests.Count().ToString())
            };

            return JsonData.ToString();
        }

    }

   
}
