using TaskPlusPlus.API.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using TaskPlusPlus.API.DbContexts;

namespace TaskPlusPlus.API.Services
{
    public partial class TaskPlusPlusRepository : ITaskPlusPlusRepository, IDisposable
    {
        public async Task<string> GetBoardsAsync(string accessToken)
        {
            var user = await GetUserSessionAsync(accessToken);

            var res = from board in context.Boards.Where(b => !b.Deleted).OrderBy(b => b.CreationAt)
                      join sharedBoard in context.SharedBoards
                      .Where(shared => shared.ShareTo == user.UserId && !shared.Deleted)
                      on board.Id equals sharedBoard.BoardId
                      select new
                      {
                          board.Id,
                          board.CreatorId,
                          board.Caption,
                          board.CreationAt,
                      };

            var jsonData = new JArray();
            foreach (var item in res)
            {
                var childs = GetBoardsAllChilds(item.Id);
                jsonData.Add(new JObject
                {
                    {"Id", item.Id },
                    {"Creator",  (await GetUser(item.CreatorId)).FirstName },
                    {"Caption",  item.Caption },
                    {"CreationAt",  item.CreationAt },
                    {"ChildsCount", childs.Length },
                    {"CommentsCount", GetBoardsCommentsCount(childs)},
                    {"EmployeesCount", await GetEmployeesCount(item.Id) },
                    {"IsOwner", await IsOwnerOfBoardAsync(user.UserId,item.Id)}
                });
            }

            return jsonData.ToString();
        }

        public async Task<JObject> AddBoardAsync(string accessToken, string caption)
        {            
            var user = await GetUserSessionAsync(accessToken);

            var board = new Board()
            {
                Id = Guid.NewGuid(),
                CreationAt = DateTime.Now,
                Caption = caption,
                CreatorId = user.UserId,
                Deleted = false
            };
            await context.Boards.AddAsync(board);

            // add to shareTo for the user
            var shareTo = new SharedBoard()
            {
                ShareTo = user.UserId,
                BoardId = board.Id
            };
            await context.SharedBoards.AddAsync(shareTo);
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<JObject> UpdateBoardAsync(string accessToken, Guid boardId, string caption)
        {            
            var user = await GetUserSessionAsync(accessToken);

            // check accessibility
            if (!await IsOwnerOfBoardAsync(user.UserId, boardId))
                return JsonMap.FalseResult;

            var board = await context.Boards.SingleAsync(b => b.Id == boardId);
            board.Caption = caption;
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<JObject> DeleteBoardAsync(string accessToken, Guid boardId)
        {            
            var user = await GetUserSessionAsync(accessToken);
            if (!await context.Boards.AnyAsync(b => b.Id == boardId && b.CreatorId == user.UserId)) return JsonMap.FalseResult;

            var board = await context.Boards.SingleAsync(b => b.Id == boardId);
            board.Deleted = true;
            await context.SaveChangesAsync();
            return JsonMap.TrueResult;
        }

        public async Task<JObject> ShareBoardAsync(string accessToken, Guid boardId, Guid[] shareToList)
        {            
            var user = await GetUserSessionAsync(accessToken);
            var board = await context.Boards.SingleOrDefaultAsync(b => b.Id == boardId && b.CreatorId == user.UserId);

            if (board == null) return JsonMap.FalseResult;

            foreach (var item in shareToList)
            {
                var friend = await context.Profiles.SingleAsync(f => f.UserId == item);

                if (await context.SharedBoards.AnyAsync(s => s.BoardId == boardId && s.ShareTo == friend.UserId && !s.Deleted)) continue;

                // return false if there is not any active friend ship between them
                if (!(await context.FriendLists.AnyAsync(f =>
                    (f.From == user.UserId && f.To == friend.UserId && !f.Removed && f.Accepted) ||
                    (f.To == user.UserId && f.From == friend.UserId && !f.Removed && f.Accepted))))
                    return JsonMap.FalseResult;

                var share = new SharedBoard()
                {
                    Id = Guid.NewGuid(),
                    BoardId = boardId,
                    ShareTo = friend.UserId,
                    GrantedAccessAt = DateTime.Now,
                    Deleted = false
                };

                await context.SharedBoards.AddAsync(share);
            }

            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }


        private async Task<bool> IsOwnerOfBoardAsync(Guid userId, Guid parentId)
        {            
            parentId = await GetMainBoardId(parentId);

            return await context.Boards.AnyAsync(b => b.Id == parentId && b.CreatorId == userId);
        }

        private async Task<Guid> GetBoardIdAsync(Guid parentId)
        {            
            var boardId = await GetMainBoardId(parentId);

            return boardId;
        }

        private Guid[] GetBoardsAllChilds(Guid boardId)
        {            
            var tasksList = new List<Guid>();

            var tasks = from task in context.Tasks.Where(t => t.ParentId == boardId && !t.Deleted)
                        select new
                        {
                            task.Id
                        };

            tasksList.AddRange(tasks.Select(data => data.Id));

            for (var i = 0; i < tasksList.Count; i++)
            {
                var subTasks = from subTask in context.Tasks.Where(t => t.ParentId == tasksList[i] && !t.Deleted)
                               select new
                               {
                                   subTask.Id,
                               };

                tasksList.AddRange(subTasks.Select(data => data.Id));
            }

            return tasksList.ToArray();
        }

        private int GetBoardsCommentsCount(Guid[] childs)
        {            
            var commentsList = new List<Guid>();
            foreach (var item in childs)
            {
                var comments = from comment in context.Comments.Where(c => c.ParentId == item && c.Id == c.EditId && !c.Deleted)
                               select new
                               {
                                   comment.Id
                               };

                commentsList.AddRange(comments.Select(c => c.Id));
            }

            return commentsList.Count;
        }

        private async Task<int> GetEmployeesCount(Guid boardId)
        {            
            return await context.SharedBoards.CountAsync(s => !s.Deleted && s.BoardId == boardId && !s.Deleted);
        }

        public async Task<JObject> RemoveBoardShareAsync(string accessToken, Guid boardId, Guid shareId)
        {            
            var user = await GetUserSessionAsync(accessToken);

            if (!(await IsOwnerOfBoardAsync(user.UserId, boardId))) return JsonMap.FalseResult;

            var share = await context.SharedBoards.SingleOrDefaultAsync(s => s.Id == shareId && s.ShareTo != user.UserId);

            share.Deleted = true;

            await context.SaveChangesAsync();
            return JsonMap.TrueResult;
        }
    }
}
