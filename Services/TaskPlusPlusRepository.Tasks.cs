using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskPlusPlus.API.DbContexts;

namespace TaskPlusPlus.API.Services
{
    public partial class TaskPlusPlusRepository
    {
        public async Task<string> GetTasksAsync(string accessToken, Guid parentId)
        {
            var jsonData = new JArray();
            var LastModifiedByUsers = new List<Guid>();

            Logger.Log("\n\n\n\nStart");
            var user = await GetUserSessionAsync(accessToken);
            Logger.Log($"{1}\n");
            var boardId = await GetBoardIdAsync(parentId);
            Logger.Log($"{2}\n");
            var isOwner = await IsOwnerOfBoardAsync(user.UserId, parentId);
            Logger.Log($"{3}\n");

            if (!(await HaveAccessToTaskَAsync(user.UserId, boardId))) return jsonData.ToString();
            Logger.Log($"{4}\n");
            if (!isOwner && !(await HasRoleAccess(boardId, user.UserId, Permissions.ReadTask))) return jsonData.ToString();
            Logger.Log($"{5}\n");
            var HasTagAccessList = new List<bool>();
            Logger.Log($"{6}\n");
            using (var context = new TaskPlusPlusContext())
            {
                var res = from task in context.Tasks
                          .Where(t => t.ParentId == parentId && !t.Deleted).OrderBy(t => t.CreationAt)
                          select new
                          {
                              task.Id
                          };
                foreach (var item in res)
                {
                    HasTagAccessList.Add(await HasTagAccess(boardId, user.UserId, item.Id));
                }
            }
            Logger.Log($"{7}\n");
            using (var context = new TaskPlusPlusContext())
            {
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
                var counter = 0;
                foreach (var item in res)
                {
                    if (!isOwner && !(HasTagAccessList[counter++]))
                        continue;

                    LastModifiedByUsers.Add(item.LastModifiedBy);
                    jsonData.Add(new JObject
                    {
                        {"Id", item.Id },
                        {"Caption",  item.Caption },
                        {"Star",  item.Star },
                        {"CreationAt",  item.CreationAt },
                        {"LastModifiedBy", string.Empty },
                        {"Tags", string.Empty },
                        {"Compeleted" , item.Compeleted},
                        {"SubTasksCount" , string.Empty},
                        {"SubCommentsCount" , string.Empty}
                    });
                }
            }
            Logger.Log($"{8}\n");
            var index = 0;
            foreach (var item in jsonData)
            {
                Guid id = Guid.Parse(item["Id"].ToString());
                Logger.Log($"{9_1}\n");
                item["LastModifiedBy"] = (await GetUser(LastModifiedByUsers[index])).FirstName;
                Logger.Log($"{9_2}\n");
                item["Tags"] = JToken.FromObject(await GetTaskTagListAsync(id));
                Logger.Log($"{9_3}\n");
                item["SubTasksCount"] = await GetChildsCount(id);
                Logger.Log($"{9_4}\n");
                item["SubCommentsCount"] = await GetCommentsCount(id);
                Logger.Log($"{9_5}\n");
            }
            return jsonData.ToString();
        }

        public async Task<JObject> AddTaskAsync(string accessToken, Guid parentId, string caption)
        {
            using var context = new TaskPlusPlusContext();
            var user = await GetUserSessionAsync(accessToken, context);
            if (!await HaveAccessToTaskَAsync(user.UserId, parentId, context)) return JsonMap.FalseResult;

            var board = await context.Boards.SingleAsync(b => b.Id == parentId);
            var isOwner = await IsOwnerOfBoardAsync(user.UserId, parentId, context);

            if (!isOwner && !(await HasPermissionsAsync(user.UserId, parentId, Permissions.WriteTask, context))) return JsonMap.FalseResult;

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
            using var context = new TaskPlusPlusContext();
            var user = await GetUserSessionAsync(accessToken, context);
            if (!await HaveAccessToTaskَAsync(user.UserId, parentId, context)) return JsonMap.FalseResult;

            var isOwner = await IsOwnerOfBoardAsync(user.UserId, parentId, context);
            if (!isOwner && !(await HasPermissionsAsync(user.UserId, parentId, Permissions.WriteTask, context))) return JsonMap.FalseResult;

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
            using var context = new TaskPlusPlusContext();
            var user = await GetUserSessionAsync(accessToken, context);
            if (!await HaveAccessToTaskَAsync(user.UserId, parentId, context)) return JsonMap.FalseResult;

            var isOwner = await IsOwnerOfBoardAsync(user.UserId, parentId, context);
            if (!isOwner && !await HasPermissionsAsync(user.UserId, parentId, Permissions.WriteTask, context)) return JsonMap.FalseResult;

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
            using var context = new TaskPlusPlusContext();
            var user = await GetUserSessionAsync(accessToken, context);
            if (!await HaveAccessToTaskَAsync(user.UserId, parentId, context)) return JsonMap.FalseResult;

            var isOwner = await IsOwnerOfBoardAsync(user.UserId, parentId, context);
            if (!isOwner && !await HasPermissionsAsync(user.UserId, parentId, Permissions.WriteTask, context)) return JsonMap.FalseResult;

            var task = await context.Tasks.SingleOrDefaultAsync(t => t.Id == parentId && (t.Creator == user.UserId || isOwner));
            if (task == null) return JsonMap.FalseResult;

            task.Caption = caption;
            task.Star = star;
            task.LastModifiedBy = user.UserId;

            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        private static async Task<bool> HaveAccessToTaskَAsync(Guid userId, Guid parentId, TaskPlusPlusContext context)
        {
            parentId = await GetMainBoardId(parentId);

            var board = await context.Boards.SingleAsync(b => b.Id == parentId);
            return await context.SharedBoards.AnyAsync(s => s.BoardId == board.Id && s.ShareTo == userId && !s.Deleted);
        }

        public static async Task<Guid> GetMainBoardId(Guid parentId)
        {
            using var context = new TaskPlusPlusContext();

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
            using var context = new TaskPlusPlusContext();
            var user = await GetUserSessionAsync(accessToken, context);
            if (!await HaveAccessToTaskَAsync(user.UserId, parentId, context)) return JsonMap.FalseResult;
            if (!await HasRoleAccess(parentId, user.UserId, Permissions.CompeleteTask, context)) return JsonMap.FalseResult; //todo: check // added by mul83rry


            var isOwner = await IsOwnerOfBoardAsync(user.UserId, parentId, context);
            var task = await context.Tasks.SingleOrDefaultAsync(t => t.Id == parentId && isOwner);
            if (task == null) return JsonMap.FalseResult;

            task.Compeleted = true;

            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<JObject> DeleteTaskAsync(string accessToken, Guid parentId)
        {
            using var context = new TaskPlusPlusContext();
            var user = await GetUserSessionAsync(accessToken, context);
            if (!await HaveAccessToTaskَAsync(user.UserId, parentId, context)) return JsonMap.FalseResult;

            var isOwner = await IsOwnerOfBoardAsync(user.UserId, parentId, context);
            var task = await context.Tasks.SingleOrDefaultAsync(t => t.Id == parentId && (t.Creator == user.UserId || isOwner));

            if (task == null) return JsonMap.FalseResult;
            if (!isOwner && !await HasPermissionsAsync(user.UserId, parentId, Permissions.WriteTask, context)) return JsonMap.FalseResult;

            task.Deleted = true;
            task.LastModifiedBy = user.UserId;
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        private async static Task<int> GetChildsCount(Guid parentId)
        {
            return await context.Tasks.CountAsync(t => t.ParentId == parentId && !t.Deleted);
        }

        private async static Task<int> GetCommentsCount(Guid parentId, TaskPlusPlusContext context)
        {
            return await context.Comments.CountAsync(c => c.ParentId == parentId && c.Id == c.EditId && !c.Deleted);
        }
    }
}
