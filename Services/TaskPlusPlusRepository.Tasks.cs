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
        public async Task<string> GetTasksAsync(string accessToken, Guid parentId)
        {

            var user = await GetUserSessionAsync(accessToken);
            var boardId = await GetBoardIdAsync(parentId);
            var isOwner = await IsOwnerOfBoardAsync(user.UserId, parentId);

            var jsonData = new JArray();
            // todo: switch to signalR
            if (!(await HaveAccessToTaskَAsync(user.UserId, boardId))) return jsonData.ToString();
            if (!isOwner && !(await HasRoleAccess(boardId, user.UserId, Permissions.ReadTask))) return jsonData.ToString();

            var res = from task in context.Tasks
                      .Where(t => t.ParentId == parentId && !t.Deleted).OrderBy(t => t.CreationAt)
                      select new
                      {
                          task.Id,
                          task.Caption,
                          task.Star,
                          task.CreationAt,
                          task.LastModifiedBy,
                          task.Compeleted
                      };

            // todo: can be simplest
            foreach (var item in res)
            {
                if (!isOwner && !(await HasTagAccess(boardId, user.UserId, item.Id)))
                    continue;

                jsonData.Add(new JObject
                    {
                        {"Id", item.Id },
                        {"Caption",  item.Caption },
                        {"Star",  item.Star },
                        {"CreationAt",  item.CreationAt },
                        {"LastModifiedBy", (await GetUser(item.LastModifiedBy)).FirstName.ToString()},
                        {"Tags", JToken.FromObject(await GetTaskTagListAsync(item.Id))},
                        {"Compeleted" , item.Compeleted},
                        {"SubTasksCount" , await GetChildsCount(item.Id)},
                        {"SubCommentsCount" , await GetCommentsCount(item.Id)},
                        {"DeleteLicense", isOwner || await context.Tasks.AnyAsync(t => t.Id == item.Id && t.Creator == user.UserId)},
                        {"EditLicense", isOwner || await HasPermissionsAsync(user.UserId, item.Id, Permissions.WriteTask)}
                    });
            }
            return jsonData.ToString();
        }

        public async Task<JObject> AddTaskAsync(string accessToken, Guid parentId, string caption)
        {

            var user = await GetUserSessionAsync(accessToken);
            if (!await HaveAccessToTaskَAsync(user.UserId, parentId)) return JsonMap.FalseResult;

            var board = await context.Boards.SingleAsync(b => b.Id == parentId);
            var isOwner = await IsOwnerOfBoardAsync(user.UserId, parentId);

            if (!isOwner && !(await HasPermissionsAsync(user.UserId, parentId, Permissions.WriteTask))) return JsonMap.FalseResult;

            var task = new Entities.Task()
            {
                Id = Guid.NewGuid(),
                Caption = caption,
                ParentId = board.Id,
                Star = false,
                CreationAt = DateTime.Now,
                Deleted = false,
                Creator = user.UserId,
                LastModifiedBy = user.UserId,
                Compeleted = false,
                Status = Settings.MainList
            };
            // todo: rename 'Stutus' to List

            await context.Tasks.AddAsync(task);
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<JObject> AddSubTaskAsync(string accessToken, Guid parentId, string caption)
        {

            var user = await GetUserSessionAsync(accessToken);
            if (!await HaveAccessToTaskَAsync(user.UserId, parentId)) return JsonMap.FalseResult;

            var isOwner = await IsOwnerOfBoardAsync(user.UserId, parentId);
            if (!isOwner && !(await HasPermissionsAsync(user.UserId, parentId, Permissions.WriteTask))) return JsonMap.FalseResult;

            var task = await context.Tasks.SingleAsync(t => t.Id == parentId);

            var subTask = new Entities.Task()
            {
                Id = Guid.NewGuid(),
                Caption = caption,
                ParentId = task.Id,
                Star = false,
                CreationAt = DateTime.Now,
                Deleted = false,
                Creator = user.UserId,
                LastModifiedBy = user.UserId,
                Compeleted = false,
                Status = Settings.MainList,
            };

            await context.Tasks.AddAsync(subTask);
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<JObject> EditTaskAsync(string accessToken, Guid parentId, string caption, bool star)
        {

            var user = await GetUserSessionAsync(accessToken);
            if (!await HaveAccessToTaskَAsync(user.UserId, parentId)) return JsonMap.FalseResult;

            var isOwner = await IsOwnerOfBoardAsync(user.UserId, parentId);
            if (!isOwner && !await HasPermissionsAsync(user.UserId, parentId, Permissions.WriteTask)) return JsonMap.FalseResult;

            var task = await context.Tasks.SingleOrDefaultAsync(t => t.Id == parentId && (t.Creator == user.UserId || isOwner));
            if (task == null) return JsonMap.FalseResult;

            task.Caption = caption;
            task.Star = star;
            task.LastModifiedBy = user.UserId;

            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<JObject> EditSubTaskAsync(string accessToken, Guid parentId, string caption, bool star)
        {

            var user = await GetUserSessionAsync(accessToken);
            if (!await HaveAccessToTaskَAsync(user.UserId, parentId)) return JsonMap.FalseResult;

            var isOwner = await IsOwnerOfBoardAsync(user.UserId, parentId);
            if (!isOwner && !await HasPermissionsAsync(user.UserId, parentId, Permissions.WriteTask)) return JsonMap.FalseResult;

            var task = await context.Tasks.SingleOrDefaultAsync(t => t.Id == parentId && (t.Creator == user.UserId || isOwner));
            if (task == null) return JsonMap.FalseResult;

            task.Caption = caption;
            task.Star = star;
            task.LastModifiedBy = user.UserId;

            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        private async Task<bool> HaveAccessToTaskَAsync(Guid userId, Guid parentId)
        {

            parentId = await GetMainBoardId(parentId);

            var board = await context.Boards.SingleAsync(b => b.Id == parentId);
            return await context.SharedBoards.AnyAsync(s => s.BoardId == board.Id && s.ShareTo == userId && !s.Deleted);
        }

        public async Task<Guid> GetMainBoardId(Guid parentId)
        {


            while (true)
            {
                var tempTask = await context.Tasks.SingleOrDefaultAsync(t => t.Id == parentId);

                if (tempTask == null) break;
                parentId = tempTask.ParentId;
            }

            return parentId;
        }

        public async Task<JObject> CompeleteTaskAsync(string accessToken, Guid parentId)
        {

            var user = await GetUserSessionAsync(accessToken);
            if (!await HaveAccessToTaskَAsync(user.UserId, parentId)) return JsonMap.FalseResult;
            if (!await HasRoleAccess(parentId, user.UserId, Permissions.CompeleteTask)) return JsonMap.FalseResult; //todo: check // added by mul83rry


            var isOwner = await IsOwnerOfBoardAsync(user.UserId, parentId);
            var task = await context.Tasks.SingleOrDefaultAsync(t => t.Id == parentId && isOwner);
            if (task == null) return JsonMap.FalseResult;

            task.Compeleted = true;

            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<JObject> DeleteTaskAsync(string accessToken, Guid parentId)
        {

            var user = await GetUserSessionAsync(accessToken);
            if (!await HaveAccessToTaskَAsync(user.UserId, parentId)) return JsonMap.FalseResult;

            var isOwner = await IsOwnerOfBoardAsync(user.UserId, parentId);
            var task = await context.Tasks.SingleOrDefaultAsync(t => t.Id == parentId && (t.Creator == user.UserId || isOwner));

            if (task == null) return JsonMap.FalseResult;
            if (!isOwner && !await HasPermissionsAsync(user.UserId, parentId, Permissions.WriteTask)) return JsonMap.FalseResult;

            task.Deleted = true;
            task.LastModifiedBy = user.UserId;
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        private async Task<int> GetChildsCount(Guid parentId)
        {

            return await context.Tasks.CountAsync(t => t.ParentId == parentId && !t.Deleted);
        }

        private async Task<int> GetCommentsCount(Guid parentId)
        {

            return await context.Comments.CountAsync(c => c.ParentId == parentId && c.Id == c.EditId && !c.Deleted);
        }
    }
}
