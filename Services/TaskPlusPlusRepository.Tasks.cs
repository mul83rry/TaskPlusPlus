using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TaskPlusPlus.API.Services
{
    public partial class TaskPlusPlusRepository
    {
        public async Task<string> GetTasksAsync(string accessToken, Guid parentId)
        {
            var user = await GetUserSessionAsync(accessToken);
            var boardId = await GetBoardIdAsync(parentId);
            var isOwner = await IsOwnerOfBoard(user.UserId, parentId);

            var jsonData = new JArray();
            // todo: switch to signalR
            if (!(await HaveAccessToTask(user.UserId, boardId))) return jsonData.ToString();
            if (!isOwner && !await HasRoleAccess(boardId, user.UserId, Permissions.ReadTask)) return jsonData.ToString();


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
                if (!isOwner && !await HasTagAccess(boardId, user.UserId, item.Id))
                    continue;

                jsonData.Add(new JObject
                    {
                        {"Id", item.Id },
                        {"Caption",  item.Caption },
                        {"Star",  item.Star },
                        {"CreationAt",  item.CreationAt },
                        {"LastModifiedBy", (await GetUser(item.LastModifiedBy)).FirstName.ToString()},
                        {"Tags", (await GetTaskTagListAsync(item.Id))},
                        {"Compeleted" , item.Compeleted},
                        {"SubTasksCount" , GetChildsCount(item.Id)},
                        {"SubCommentsCount" , GetCommentsCount(item.Id)},
                    });
            }
            return jsonData.ToString();
        }

        public async Task<JObject> AddTaskAsync(string accessToken, Guid parentId, string caption)
        {
            var user = await GetUserSessionAsync(accessToken);
            var board = await context.Boards.SingleAsync(b => b.Id == parentId);
            var isOwner = await IsOwnerOfBoard(user.UserId, parentId);


            // check accessibility
            if (!await HaveAccessToTask(user.UserId, parentId)) return JsonMap.FalseResult;

            if (!isOwner && !(await HasPermissions(user.UserId, parentId, Permissions.WriteTask))) return JsonMap.FalseResult;

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
                Status = "ToDo",

            };

            await context.Tasks.AddAsync(task);
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<JObject> AddSubTaskAsync(string accessToken, Guid parentId, string caption)
        {
            var user = await GetUserSessionAsync(accessToken);
            var isOwner = await IsOwnerOfBoard(user.UserId, parentId);

            // check accessibility
            if (!await HaveAccessToTask(user.UserId, parentId)) return JsonMap.FalseResult;

            var task = await context.Tasks.SingleAsync(t => t.Id == parentId);
            if (!isOwner && !(await HasPermissions(user.UserId, parentId, Permissions.WriteTask))) return JsonMap.FalseResult;

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
            };

            await context.Tasks.AddAsync(subTask);
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<JObject> EditTaskAsync(string accessToken, Guid parentId, string caption, bool star)
        {
            var user = await GetUserSessionAsync(accessToken);
            var isOwner = await IsOwnerOfBoard(user.UserId, parentId);
            var task = await context.Tasks.SingleOrDefaultAsync(t => t.Id == parentId && (t.Creator == user.UserId || isOwner));

            if (task == null) return JsonMap.FalseResult;

            // check accessibility
            if (!await HaveAccessToTask(user.UserId, parentId)) return JsonMap.FalseResult;
            if (!isOwner && !await HasPermissions(user.UserId, parentId, Permissions.WriteTask)) return JsonMap.FalseResult;

            task.Caption = caption;
            task.Star = star;
            task.LastModifiedBy = user.UserId;

            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<JObject> EditSubTaskAsync(string accessToken, Guid parentId, string caption, bool star)
        {
            var user = await GetUserSessionAsync(accessToken);
            var isOwner = await IsOwnerOfBoard(user.UserId, parentId);
            var task = await context.Tasks.SingleOrDefaultAsync(t => t.Id == parentId && (t.Creator == user.UserId || isOwner));

            if (task == null) return JsonMap.FalseResult;

            if (!await HaveAccessToTask(user.UserId, parentId)) return JsonMap.FalseResult;
            if (!isOwner && !await HasPermissions(user.UserId, parentId, Permissions.WriteTask)) return JsonMap.FalseResult;

            task.Caption = caption;
            task.Star = star;
            task.LastModifiedBy = user.UserId;
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }



        private async Task<bool> HaveAccessToTask(Guid userId, Guid parentId)
        {
            var pId = parentId;
            while (true)
            {
                var tempTask = await context.Tasks.SingleOrDefaultAsync(t => t.Id == pId);

                if (tempTask == null) break;
                pId = tempTask.ParentId;
            }

            var board = await context.Boards.SingleAsync(b => b.Id == pId);
            return await context.SharedBoards.AnyAsync(s => s.BoardId == board.Id && s.ShareTo == userId);
        }

        public async Task<JObject> CompeleteTaskAsync(string accessToken, Guid parentId)
        {
            var user = await GetUserSessionAsync(accessToken);
            var isOwner = await IsOwnerOfBoard(user.UserId, parentId);
            var task = await context.Tasks.SingleOrDefaultAsync(t => t.Id == parentId && isOwner);

            if (task == null) return JsonMap.FalseResult;
            if (!await HaveAccessToTask(user.UserId, parentId)) return JsonMap.FalseResult;


            task.Compeleted = true;
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<JObject> DeleteTaskAsync(string accessToken, Guid parentId)
        {
            var user = await GetUserSessionAsync(accessToken);
            var isOwner = await IsOwnerOfBoard(user.UserId, parentId);
            var task = await context.Tasks.SingleOrDefaultAsync(t => t.Id == parentId && (t.Creator == user.UserId || isOwner));

            if (task == null) return JsonMap.FalseResult;
            if (!await HaveAccessToTask(user.UserId, parentId)) return JsonMap.FalseResult;
            if (!isOwner && !await HasPermissions(user.UserId, parentId, Permissions.WriteTask)) return JsonMap.FalseResult;

            task.Deleted = true;
            task.LastModifiedBy = user.UserId;
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }
        //private async Task<JObject> HaveChild(Guid taskId) => new JObject { { "result", await context.Tasks.AnyAsync(t => t.ParentId == taskId && t.Deleted == false) } };

        private int GetChildsCount(Guid parentId)
        {
            var tasks = from task in context.Tasks.Where(t => t.ParentId == parentId && !t.Deleted)
                        select new
                        {
                            task.Id
                        };



            return tasks.Count();
        }


        private int GetCommentsCount(Guid parentId)
        {
            var comments = from comment in context.Comments.Where(c => c.ParentId == parentId && c.Id == c.EditId && !c.Deleted)
                           select new
                           {
                               comment.Id
                           };



            return comments.Count();
        }
    }
}
