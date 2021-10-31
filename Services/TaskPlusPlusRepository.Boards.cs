using TaskPlusPlus.API.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TaskPlusPlus.API.Services
{
    public partial class TaskPlusPlusRepository : ITaskPlusPlusRepository, IDisposable
    {
        public async Task<string> GetBoardsAsync(string accessToken)
        {
            var user = await GetUserSessionAsync(accessToken);

            var res = from board in context.Boards.Where(b => !b.Deleted)
                      join sharedBoard in context.SharedBoards
                      .Where(shared => shared.ShareTo == user.UserId).OrderBy(s => s.GrantedAccessAt)
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
                jsonData.Add(new JObject
                {
                    {"id", item.Id },
                    {"CreatorId",  item.CreatorId },
                    {"caption",  item.Caption },
                    {"creationAt",  item.CreationAt }
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
            if (!await IsOwnerOfBoard(user.UserId, boardId))
                return JsonMap.FalseResult;

            var board = await context.Boards.SingleAsync(b => b.Id == boardId);
            board.Caption = caption;
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<JObject> DeleteBoardAsync(string accessToken, Guid boardId)
        {
            var user = await GetUserSessionAsync(accessToken);
            // todo: check
            // check accessibility
            if (!await context.Boards.AnyAsync(b => b.Id == boardId && b.CreatorId == user.UserId)) return JsonMap.FalseResult;

            var board = await context.Boards.SingleAsync(b => b.Id == boardId);
            board.Deleted = true;
            await context.SaveChangesAsync();
            return JsonMap.TrueResult;
        }

        public async Task<JObject> ShareBoardAsync(string accessToken, Guid boardId, string shareToList)
        {
            var user = await GetUserSessionAsync(accessToken);
            var board = await context.Boards.SingleAsync(b => b.Id == boardId && b.CreatorId == user.UserId);

            var usersIdToShare = shareToList.Split(',');
            foreach (var item in usersIdToShare.Where(i => !string.IsNullOrEmpty(i)))
            {
                var friend = await context.Profiles.SingleAsync(u => u.UserId == Guid.Parse(item));

                if (await context.SharedBoards.AnyAsync(s => s.BoardId == boardId && s.ShareTo == friend.Id)) continue;

                if (!(await context.FriendLists.AnyAsync(f =>
                (f.From == user.UserId && f.To == friend.Id && !f.Removed && f.Accepted) ||
                (f.To == user.UserId && f.From == friend.Id && !f.Removed && f.Accepted))))
                    return JsonMap.FalseResult;

                var share = new SharedBoard()
                {
                    Id = Guid.NewGuid(),
                    BoardId = boardId,
                    ShareTo = friend.Id,
                    GrantedAccessAt = DateTime.Now,
                };

                await context.SharedBoards.AddAsync(share);
            }

            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }


        private async Task<bool> IsOwnerOfBoard(Guid userId, Guid parentId)
        {
            var pId = parentId;
            var tempTask = new Entities.Task();

            // serach for main board id
            while (true)
            {
                tempTask = await context.Tasks.SingleOrDefaultAsync(t => t.Id == pId);

                if (tempTask == null) break;
                pId = tempTask.ParentId;
            }

            return await context.Boards.AnyAsync(b => b.Id == pId && b.CreatorId == userId);
        }

        private async Task<Guid> GetBoardIdAsync(Guid parentId)
        {
            var boardId = parentId;
            var tempTask = new Entities.Task();
            while (true)
            {
                tempTask = await context.Tasks.SingleOrDefaultAsync(t => t.Id == boardId);

                if (tempTask == null) break;
                boardId = tempTask.ParentId;
            }

            return boardId;
        }

    }
}
