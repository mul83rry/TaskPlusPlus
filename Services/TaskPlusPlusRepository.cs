using TaskPlusPlus.API.DbContexts;
using TaskPlusPlus.API.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TaskPlusPlus.API.Services
{
    enum Permissions
    {
        readTask,
        writeTask,
        readComment,
        writeComment
    }

    public class TaskPlusPlusRepository : ITaskPlusPlusRepository, IDisposable
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

        private async Task<Session> GetUserSessionAsync(string accessToken) => string.IsNullOrEmpty(accessToken)
            ? throw new ArgumentNullException("not valid argument")
            : await context.Sessions.SingleAsync(s => s.AccessToken == accessToken);

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
            if (!(await IsOwner(user.UserId, boardId)))
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

        public async Task<JObject> SigninAsync(string phoneNumber)
        {
            if (!await context.Users.AnyAsync(u => u.PhoneNumber == phoneNumber))
                return JsonMap.FalseResultWithEmptyAccessToken;

            var user = await context.Users.SingleAsync(u => u.PhoneNumber == phoneNumber);

            // Add new session
            var newSession = new Session()
            {
                AccessToken = Guid.NewGuid().ToString(),
                Id = Guid.NewGuid(),
                IsValid = true,
                UserId = user.Id,
                CreationAt = DateTime.Now,
                LastFetchTime = DateTime.Now - TimeSpan.FromHours(1)
            };

            await context.Sessions.AddAsync(newSession);
            await context.SaveChangesAsync();
            return JsonMap.GetSuccesfullAccessToken(newSession.AccessToken);
        }

        public async Task<JObject> SignUpAsync(string firstName, string lastName, string phoneNumber)
        {
            if (await context.Users.AnyAsync(u => u.PhoneNumber == phoneNumber))
                return JsonMap.FalseResultWithEmptyAccessToken;

            var newUser = new User()
            {
                Id = Guid.NewGuid(),
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber,
                SignupDate = DateTime.Now
            };
            await context.Users.AddAsync(newUser);

            // Add new session
            var newSession = new Session()
            {
                AccessToken = Guid.NewGuid().ToString(),
                Id = Guid.NewGuid(),
                IsValid = true,
                UserId = newUser.Id,
                CreationAt = DateTime.Now
            };

            await context.Sessions.AddAsync(newSession);
            await context.SaveChangesAsync();
            return JsonMap.GetSuccesfullAccessToken(newSession.AccessToken);
        }

        public async Task<string> GetTasksAsync(string accessToken, Guid parentId)
        {
            var user = await GetUserSessionAsync(accessToken);
            var boardId = await GetBoardIdAsync(parentId);
            var isOwner = await IsOwner(user.UserId, parentId);

            var jsonData = new JArray();
            // todo: switch to signalR
            if (!(await HaveAccessToTask(user.UserId, boardId))) return jsonData.ToString();
            if (!isOwner && !await HasRoleAccess(boardId, user.UserId, Permissions.readTask)) return jsonData.ToString();


            var res = from task in context.Tasks
                      .Where(t => t.ParentId == parentId && !t.Deleted).OrderBy(t => t.CreationAt)
                      select new
                      {
                          task.Id,
                          task.Caption,
                          task.Star,
                          task.CreationAt,
                          task.LastModifiedBy
                      };

            // todo: can be simplest
            foreach (var item in res)
            {
                if (!isOwner && !await HasTagAccess(boardId, user.UserId, item.Id))
                    continue;

                jsonData.Add(new JObject
                    {
                        {"id", item.Id },
                        {"caption",  item.Caption },
                        {"star",  item.Star },
                        {"creationAt",  item.CreationAt },
                        {"haveChild", (await HaveChild(item.Id))["result"] },
                        {"lastModifiedBy", (await GetUser(item.LastModifiedBy)).FirstName.ToString()},
                        {"tags", (await GetTaskTagListAsync(item.Id))}
                    });
            }
            return jsonData.ToString();
        }

        public async Task<JObject> AddTaskAsync(string accessToken, Guid parentId, string caption)
        {
            var user = await GetUserSessionAsync(accessToken);
            var board = await context.Boards.SingleAsync(b => b.Id == parentId);
            var isOwner = await IsOwner(user.UserId, parentId);


            // check accessibility
            if (!await HaveAccessToTask(user.UserId, parentId)) return JsonMap.FalseResult;

            if (!isOwner && !(await HasPermissions(user.UserId, parentId, Permissions.writeTask))) return JsonMap.FalseResult;

            var task = new Entities.Task()
            {
                Id = Guid.NewGuid(),
                Caption = caption,
                ParentId = board.Id,
                Star = false,
                CreationAt = DateTime.Now,
                Deleted = false,
                Creator = user.UserId,
                LastModifiedBy = user.UserId
            };

            await context.Tasks.AddAsync(task);
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<JObject> AddSubTaskAsync(string accessToken, Guid parentId, string caption)
        {
            var user = await GetUserSessionAsync(accessToken);
            var isOwner = await IsOwner(user.UserId, parentId);

            // check accessibility
            if (!await HaveAccessToTask(user.UserId, parentId)) return JsonMap.FalseResult;

            var task = await context.Tasks.SingleAsync(t => t.Id == parentId);
            if (!isOwner && !(await HasPermissions(user.UserId, parentId, Permissions.writeTask))) return JsonMap.FalseResult;

            var subTask = new Entities.Task()
            {
                Id = Guid.NewGuid(),
                Caption = caption,
                ParentId = task.Id,
                Star = false,
                CreationAt = DateTime.Now,
                Deleted = false,
                Creator = user.UserId,
                LastModifiedBy = user.UserId
            };

            await context.Tasks.AddAsync(subTask);
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<JObject> EditTaskAsync(string accessToken, Guid parentId, string caption, bool star)
        {
            var user = await GetUserSessionAsync(accessToken);
            var isOwner = await IsOwner(user.UserId, parentId);
            var task = await context.Tasks.SingleOrDefaultAsync(t => t.Id == parentId && (t.Creator == user.UserId || isOwner));

            if (task == null) return JsonMap.FalseResult;

            // check accessibility
            if (!await HaveAccessToTask(user.UserId, parentId)) return JsonMap.FalseResult;
            if (!isOwner && !await HasPermissions(user.UserId, parentId, Permissions.writeTask)) return JsonMap.FalseResult;

            task.Caption = caption;
            task.Star = star;
            task.LastModifiedBy = user.UserId;
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<JObject> EditSubTaskAsync(string accessToken, Guid parentId, string caption, bool star)
        {
            var user = await GetUserSessionAsync(accessToken);
            var isOwner = await IsOwner(user.UserId, parentId);
            var task = await context.Tasks.SingleOrDefaultAsync(t => t.Id == parentId && (t.Creator == user.UserId || isOwner));

            if (task == null) return JsonMap.FalseResult;

            if (!await HaveAccessToTask(user.UserId, parentId)) return JsonMap.FalseResult;
            if (!isOwner && !await HasPermissions(user.UserId, parentId, Permissions.writeTask)) return JsonMap.FalseResult;

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

        private async Task<bool> IsOwner(Guid userId, Guid parentId)
        {
            var pId = parentId;
            var tempTask = new Entities.Task();
            while (true)
            {
                tempTask = await context.Tasks.SingleOrDefaultAsync(t => t.Id == pId);

                if (tempTask == null) break;
                pId = tempTask.ParentId;
            }

            var board = await context.Boards.SingleOrDefaultAsync(b => b.Id == pId);
            return (userId == board.CreatorId);
        }

        public async Task<JObject> DeleteTaskAsync(string accessToken, Guid parentId)
        {
            var user = await GetUserSessionAsync(accessToken);
            var isOwner = await IsOwner(user.UserId, parentId);
            var task = await context.Tasks.SingleAsync(t => t.Id == parentId && (t.Creator == user.UserId || isOwner));

            if (task == null) return JsonMap.FalseResult;
            if (!await HaveAccessToTask(user.UserId, parentId)) return JsonMap.FalseResult;
            if (!isOwner && !await HasPermissions(user.UserId, parentId, Permissions.writeTask)) return JsonMap.FalseResult;

            task.Deleted = true;
            task.LastModifiedBy = user.UserId;
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        private async Task<JObject> HaveChild(Guid taskId) => new JObject { { "result", await context.Tasks.AnyAsync(t => t.ParentId == taskId && t.Deleted == false) } };

        public async Task<JObject> AddCommentAsync(string accessToken, Guid parentId, string text)
        {
            var user = await GetUserSessionAsync(accessToken);
            var isOwner = await IsOwner(user.UserId, parentId);
            if (await HaveAccessToTask(user.UserId, parentId) == false) return JsonMap.FalseResult;
            if (!isOwner && !(await HasPermissions(user.UserId, parentId, Permissions.writeComment))) return JsonMap.FalseResult;

            var comment = new Comment()
            {
                Id = Guid.NewGuid(),
                Text = text,
                Sender = user.UserId,
                ReplyTo = parentId,
                CreationDate = DateTime.Now,
                Deleted = false,
                EditId = "0",
                LastModifiedBy = user.UserId
            };

            await context.Comments.AddAsync(comment);
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<string> GetCommentsAsync(string accessToken, Guid parentId)
        {
            var user = await GetUserSessionAsync(accessToken);
            var isOwner = await IsOwner(user.UserId, parentId);
            if (await HaveAccessToTask(user.UserId, parentId) == false) return JsonMap.FalseResult.ToString();
            if (!isOwner && !await HasPermissions(user.UserId, parentId, Permissions.readComment)) return JsonMap.FalseResult.ToString();

            var res = from comment in context.Comments
                      .Where(c => c.ReplyTo == parentId && c.Deleted == false && c.EditId == "0").OrderBy(c => c.CreationDate)
                      select new
                      {
                          comment.Id,
                          comment.Text,
                          comment.Sender,
                          comment.CreationDate,
                          comment.LastModifiedBy
                      };

            var jsonData = new JArray();
            foreach (var item in res)
            {
                jsonData.Add(new JObject
                    {
                        {"id", item.Id },
                        {"text",  item.Text },
                        {"creationAt",  item.CreationDate },
                        { "sender",(await GetUser(item.Sender)).FirstName.ToString() },
                        { "lastModifiedBy", (await GetUser(item.LastModifiedBy)).FirstName.ToString() }
                    });
            }
            return jsonData.ToString();
        }

        private async Task<User> GetUser(Guid userId) => await context.Users.SingleOrDefaultAsync(u => u.Id == userId);

        public async Task<JObject> EditCommentAsync(string accessToken, Guid parentId, Guid commentId, string text)
        {
            var user = await GetUserSessionAsync(accessToken);
            var isOwner = await IsOwner(user.UserId, parentId);
            if (await HaveAccessToTask(user.UserId, parentId) == false) return JsonMap.FalseResult;

            //find comment => create new comment => change edit id value to new comment id => save data base

            var oldComment = await context.Comments.SingleOrDefaultAsync(c => c.Id == commentId && (c.Sender == user.UserId || isOwner));
            if (oldComment == null) return JsonMap.FalseResult;

            var comment = new Comment()
            {
                Id = Guid.NewGuid(),
                Text = text,
                Sender = oldComment.Sender,
                ReplyTo = parentId,
                CreationDate = oldComment.CreationDate,
                Deleted = false,
                EditId = "0",
                LastModifiedBy = user.UserId,
            };

            oldComment.EditId = comment.Id.ToString();

            await context.Comments.AddAsync(comment);
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<JObject> DeleteCommentAsync(string accessToken, Guid parentId, Guid commentId)
        {
            var user = await GetUserSessionAsync(accessToken);
            var isOwner = await IsOwner(user.UserId, parentId);
            if (await HaveAccessToTask(user.UserId, parentId) == false) return JsonMap.FalseResult;

            var comment = await context.Comments.SingleAsync(c => c.Id == commentId && (c.Sender == user.UserId || isOwner));

            comment.Deleted = true;
            comment.LastModifiedBy = user.UserId;
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<JObject> AddFriendAsync(string accessToken, string phoneNumber)
        {
            var user = await GetUserSessionAsync(accessToken);
            var friendUser = await context.Users.SingleOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
            if (friendUser == null) return JsonMap.FalseResult;
            if (friendUser.Id == user.UserId) return JsonMap.FalseResult;

            // if already there is an active request between these two return false
            if (await context.FriendLists.AnyAsync(f =>
            (f.From == user.UserId && f.To == friendUser.Id && (!f.Removed && f.Accepted || f.Pending)) ||
            (f.To == user.UserId && f.From == friendUser.Id && (!f.Removed && f.Accepted || f.Pending))))
                return JsonMap.FalseResult;

            var friend = new FriendList()
            {
                Id = Guid.NewGuid(),
                From = user.UserId,
                To = friendUser.Id,
                Pending = true,
                Accepted = false,
                RequestDate = DateTime.Now,
                Removed = false
            };

            await context.FriendLists.AddAsync(friend);
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<string> GetFriendsListAsync(string accessToken)
        {
            var user = await GetUserSessionAsync(accessToken);

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
                User userDetail = null;

                if (item.From == user.UserId) userDetail = await GetUser(item.To);
                else if (item.To == user.UserId) userDetail = await GetUser(item.From);

                jsonData.Add(new JObject
                    {
                        {"Id", item.Id},
                        {"firstName", userDetail.FirstName },
                        {"lastName", userDetail.LastName},
                        {"phoneNumber", userDetail.PhoneNumber}
                    });
            }

            return jsonData.ToString();
        }

        public async Task<string> GetFriendRequestQueueAsync(string accessToken)
        {
            var user = await GetUserSessionAsync(accessToken);
            var res = from FList in context.FriendLists.Where(f => user.UserId == f.To && f.Pending).OrderBy(f => f.RequestDate)
                      select new
                      {
                          FList.From,
                          FList.Id
                      };

            var jsonData = new JArray();

            foreach (var item in res)
            {
                var userDetail = await GetUser(item.From);
                jsonData.Add(new JObject
                    {
                        {"Id", item.Id},
                        {"firstName", userDetail.FirstName },
                        {"lastName", userDetail.LastName},
                    });
            }

            return jsonData.ToString();
        }

        public async Task<JObject> ApplyFriendRequestAsync(string accessToken, Guid requestId, bool reply)
        {
            var user = await GetUserSessionAsync(accessToken);
            var request = await context.FriendLists.SingleOrDefaultAsync(f => f.Id == requestId);
            if (user.UserId != request.To) return JsonMap.FalseResult;
            if (!request.Pending) return JsonMap.FalseResult;

            request.Pending = false;
            request.Accepted = reply;
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<JObject> RemoveFriendAsync(string accessToken, Guid requestId)
        {
            // change user to session
            var user = await GetUserSessionAsync(accessToken);
            var request = await context.FriendLists.SingleAsync(f => f.Id == requestId);
            if (request.From != user.UserId && request.To != user.UserId) return JsonMap.FalseResult;

            request.Removed = true;
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<JObject> ShareBoardAsync(string accessToken, Guid boardId, string shareToList)
        {
            var user = await GetUserSessionAsync(accessToken);
            var board = await context.Boards.SingleAsync(b => b.Id == boardId && b.CreatorId == user.UserId);

            var usersNumberToShare = shareToList.Split(',');
            foreach (var item in usersNumberToShare.Where(i => !string.IsNullOrEmpty(i)))
            {
                var friend = await context.Users.SingleAsync(u => u.PhoneNumber == item);
                var shareTo = friend.Id;

                if (await context.SharedBoards.AnyAsync(s => s.BoardId == boardId && s.ShareTo == shareTo)) continue;

                if (!(await context.FriendLists.AnyAsync(f =>
                (f.From == user.UserId && f.To == shareTo && !f.Removed && f.Accepted) ||
                (f.To == user.UserId && f.From == shareTo && !f.Removed && f.Accepted))))
                    return JsonMap.FalseResult;

                var share = new SharedBoard()
                {
                    Id = Guid.NewGuid(),
                    BoardId = boardId,
                    ShareTo = shareTo,
                    GrantedAccessAt = DateTime.Now,
                };

                await context.SharedBoards.AddAsync(share);
            }

            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<JObject> AddTagAsync(string accessToken, Guid boardId, string caption)
        {
            var user = await GetUserSessionAsync(accessToken);
            var board = await context.Boards.SingleOrDefaultAsync(b => b.Id == boardId && b.CreatorId == user.UserId);
            if (board == null) return JsonMap.FalseResult;

            var tag = new Tag()
            {
                Id = Guid.NewGuid(),
                BoardId = boardId,
                Caption = caption,
                Removed = false,
                CreationDate = DateTime.Now
            };

            await context.Tags.AddAsync(tag);
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<string> GetTagListAsync(string accessToken, Guid parentId)
        {
            var user = await GetUserSessionAsync(accessToken);
            var boardId = await GetBoardIdAsync(parentId);

            var res = from tag in context.Tags.OrderBy(t => t.CreationDate)
                      join sharedBoard in context.SharedBoards
                      .Where(shared => shared.ShareTo == user.UserId && boardId == shared.BoardId)
                      on tag.BoardId equals sharedBoard.BoardId
                      select new
                      {
                          tag.Id,
                          tag.Caption,
                          tag.Removed,
                      };

            var jsonData = new JArray();
            foreach (var item in res)
            {
                if (item.Removed == false)
                {
                    jsonData.Add(new JObject
                    {
                        {"id", item.Id },
                        {"caption",  item.Caption }
                    });
                }

            }
            return jsonData.ToString();
        }

        public async Task<JObject> AsignTagToTaskAsync(string acessToken, Guid taskId, Guid tagId)
        {
            var user = await GetUserSessionAsync(acessToken);
            if (!await IsOwner(user.UserId, taskId)) return JsonMap.FalseResult;
            if (!await context.Tags.AnyAsync(t => t.Id == tagId && !t.Removed)) return JsonMap.FalseResult;
            if (await context.TagsList.AnyAsync(t => !t.Removed && t.TagId == tagId && t.TaskId == taskId)) return JsonMap.FalseResult;

            var taskTag = new TagsList()
            {
                Id = Guid.NewGuid(),
                TagId = tagId,
                TaskId = taskId,
                Removed = false,
                AsignDate = DateTime.Now
            };

            await context.TagsList.AddAsync(taskTag);
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        private async Task<JArray> GetTaskTagListAsync(Guid taskId)
        {
            var res = from tagList in context.TagsList.Where(t => t.TaskId == taskId).OrderBy(t => t.AsignDate)
                      select new
                      {
                          tagList.Id,
                          tagList.TagId,
                          tagList.Removed
                      };

            var jsonData = new JArray();
            foreach (var item in res)
            {
                if (item.Removed == false)
                {
                    jsonData.Add(new JObject
                    {
                        {"id", item.Id },
                        {"caption",  (await context.Tags.SingleOrDefaultAsync(t => t.Id == item.TagId)).Caption }
                    });
                }
            }

            return jsonData;
        }

        public async Task<JObject> RemoveTagFromTaskAsync(string accessToken, Guid taskId, Guid taskTagId)
        {
            var user = await GetUserSessionAsync(accessToken);
            if (!(await IsOwner(user.UserId, taskId))) return JsonMap.FalseResult;

            var taskTag = await context.TagsList.SingleOrDefaultAsync(t => t.Id == taskTagId && !t.Removed);
            if (taskTag == null) return JsonMap.FalseResult;

            taskTag.Removed = true;
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<JObject> AddRoleAsync(string accessToken, Guid boardId, string caption, bool readTask, bool writeTask, bool readComment, bool writeComment, string tagList)
        {
            var user = await GetUserSessionAsync(accessToken);
            if (!(await context.Boards.AnyAsync(b => b.Id == boardId && b.CreatorId == user.UserId))) return JsonMap.FalseResult;

            var role = new Roles()
            {
                Id = Guid.NewGuid(),
                BoardId = boardId,
                Caption = caption,
                TaskRead = readTask,
                TaskWrite = writeTask,
                CommentRead = readComment,
                CommentWrite = writeComment,
                Removed = false,
                CreatedAt = DateTime.Now
            };

            await context.Roles.AddAsync(role);
            await context.SaveChangesAsync();

            if (tagList == "none") return JsonMap.TrueResult;

            var tagListArray = tagList.Split(',');

            foreach (var item in tagListArray)
            {
                if (string.IsNullOrEmpty(item)) continue;
                if (!await context.Tags.AnyAsync(t => !t.Removed && t.Id == Guid.Parse(item) && t.BoardId == boardId)) continue;

                var roleTag = new RolesTagList()
                {
                    Id = Guid.NewGuid(),
                    RoleId = role.Id,
                    TagId = Guid.Parse(item)
                };

                await context.RolesTagList.AddAsync(roleTag);
            }

            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<string> GetBoardRolesAsync(string accessToken, Guid boardId)
        {
            var user = await GetUserSessionAsync(accessToken);

            var jsonData = new JArray();
            if (!(await context.SharedBoards.AnyAsync(s => s.ShareTo == user.UserId && s.BoardId == boardId))) return jsonData.ToString();

            var res = from roles in context.Roles.Where(r => r.BoardId == boardId && !r.Removed).OrderBy(s => s.CreatedAt)
                      select new
                      {
                          roles.Id,
                          roles.Caption,
                          roles.TaskRead,
                          roles.TaskWrite,
                          roles.CommentRead,
                          roles.CommentWrite
                      };

            foreach (var item in res)
            {
                jsonData.Add(new JObject
                {
                    {"id",item.Id},
                    {"caption",item.Caption},
                    {"readTask",item.TaskRead},
                    {"writeTask",item.TaskWrite},
                    {"readComment",item.CommentRead},
                    {"writeComment",item.CommentWrite},
                });
            }

            return jsonData.ToString();
        }

        public async Task<JObject> AsignRoleToEmployeesAsync(string accessToken, Guid boardId, Guid roleId, Guid employeesId)
        {
            var user = await GetUserSessionAsync(accessToken);
            var isOwner = await IsOwner(user.UserId, boardId);
            var selfPromote = await IsOwner(employeesId, boardId);
            var isShared = await context.SharedBoards.AnyAsync(s => s.BoardId == boardId && s.ShareTo == employeesId);
            var roleExist = await context.Roles.AnyAsync(r => r.Id == roleId && !r.Removed);

            if (!isOwner || !isShared || !roleExist || selfPromote) return JsonMap.FalseResult;

            var employeesRole = new RoleSession()
            {
                Id = Guid.NewGuid(),
                UserId = employeesId,
                RoleId = roleId,
                BoardId = boardId,
                Demoted = false,
                AsignDate = DateTime.Now
            };

            await context.RoleSessions.AddAsync(employeesRole);
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<JObject> RemoveTagAsync(string accessToken, Guid boardId, Guid tagId)
        {
            var user = await GetUserSessionAsync(accessToken);
            var isOwner = await IsOwner(user.UserId, boardId);
            var isUsing = await TagIsUsing(tagId);

            if (!isOwner || isUsing) return JsonMap.FalseResult;

            var tag = await context.Tags.SingleOrDefaultAsync(t => t.Id == tagId);
            tag.Removed = true;
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<bool> TagIsUsing(Guid tagId)
        {
            //fix check if task deleted
            var tagIsUsingInTask = false;
            var tagIsUsingInRole = false;

            var tagsInTasks = from tasksTag in context.TagsList.Where(t => t.TagId == tagId)
                              join task in context.Tasks.Where(t => !t.Deleted) on tasksTag.TaskId equals task.Id
                              select new
                              {
                                  tasksTag.Id,
                              };

            var tagsInRoles = from rolesTag in context.RolesTagList.Where(r => r.TagId == tagId)
                              join role in context.Roles.Where(r => !r.Removed) on rolesTag.RoleId equals role.Id
                              select new
                              {
                                  rolesTag.RoleId,
                              };

            if (await tagsInRoles.AnyAsync()) tagIsUsingInRole = true;
            if (await tagsInTasks.AnyAsync()) tagIsUsingInTask = true;

            return tagIsUsingInTask || tagIsUsingInRole;
        }

        public async Task<JObject> RemoveRoleFromBoardAsync(string accessToken, Guid boardId, Guid roleId)
        {
            var user = await GetUserSessionAsync(accessToken);
            if (!(await IsOwner(user.UserId, boardId))) return JsonMap.FalseResult;
            if (await context.RoleSessions.AnyAsync(r => r.RoleId == roleId && !r.Demoted)) return JsonMap.FalseResult;

            var role = await context.Roles.SingleOrDefaultAsync(r => r.Id == roleId);
            role.Removed = true;
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<JObject> DemoteEmployeesAsync(string accessToken, Guid boardId, Guid roleSessionId)
        {
            var user = await GetUserSessionAsync(accessToken);
            if (!(await IsOwner(user.UserId, boardId))) return JsonMap.FalseResult;

            var roleSession = await context.RoleSessions.SingleOrDefaultAsync(r => r.Id == roleSessionId && !r.Demoted);
            if (roleSession == null) return JsonMap.FalseResult;

            roleSession.Demoted = true;
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<string> GetEmployeesRolesAsync(string accessToken, Guid boardId)
        {
            var user = await GetUserSessionAsync(accessToken);

            var jsonData = new JArray();
            if (!await context.SharedBoards.AnyAsync(s => s.BoardId == boardId && s.ShareTo == user.UserId)) return jsonData.ToString();

            var roleSessions = from roleSession in context.RoleSessions.Where(r => r.BoardId == boardId && !r.Demoted).OrderBy(r => r.AsignDate)
                               select new
                               {
                                   roleSession.Id,
                                   roleSession.UserId,
                                   roleSession.RoleId,
                                   roleSession.AsignDate
                               };

            foreach (var item in roleSessions)
            {
                var employe = await GetUser(item.UserId);
                var role = await context.Roles.SingleOrDefaultAsync(r => r.Id == item.RoleId);

                jsonData.Add(new JObject{
                    {"id",item.Id},
                    {"firstName", employe.FirstName },
                    {"lastName", employe.LastName },
                    {"role", role.Caption },
                    {"date", item.AsignDate }
                });
            }

            return jsonData.ToString();
        }

        public async Task<string> GetEmployeesAsync(string accessToken, Guid boardId)
        {
            var user = await GetUserSessionAsync(accessToken);
            var board = await context.Boards.SingleOrDefaultAsync(b => b.Id == boardId);
            var isShared = await context.SharedBoards.AnyAsync(s => s.BoardId == boardId && s.ShareTo == user.UserId);

            var jsonData = new JArray();
            if (board == null || !isShared) return jsonData.ToString();

            var res = from sharedBoard in context.SharedBoards.Where(s => s.BoardId == boardId && s.ShareTo != board.CreatorId).OrderBy(s => s.GrantedAccessAt)
                      select new
                      {
                          sharedBoard.ShareTo,
                      };

            foreach (var item in res)
            {
                var shareTo = await GetUser(item.ShareTo);

                jsonData.Add(new JObject {
                    {"id",item.ShareTo },
                    {"firstName", shareTo.FirstName},
                    {"lastName", shareTo.LastName}
                });
            }

            return jsonData.ToString();
        }

        private async Task<bool> HasPermissions(Guid userId, Guid parentId, Permissions permissionType)
        {
            var boardId = await GetBoardIdAsync(parentId);
            var RoleAccess = await HasRoleAccess(boardId, userId, permissionType);
            var tagAccess = await HasTagAccess(boardId, userId, parentId);

            if (!RoleAccess || !tagAccess) return false;

            return true;
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

        private async Task<bool> HasRoleAccess(Guid boardId, Guid userId, Permissions permissionType)
        {
            var roles = from roleSession in context.RoleSessions.Where(r => !r.Demoted && r.BoardId == boardId && r.UserId == userId)
                        join role in context.Roles on roleSession.RoleId equals role.Id
                        select new
                        {
                            role.Id,
                            role.TaskRead,
                            role.TaskWrite,
                            role.CommentRead,
                            role.CommentWrite
                        };

            if (!(await roles.AnyAsync())) return true;

            foreach (var item in roles)
            {
                switch (permissionType)
                {
                    case Permissions.readTask:
                        if (item.TaskRead || item.TaskWrite) return true;
                        break;
                    case Permissions.writeTask:
                        if (item.TaskWrite) return true;
                        break;
                    case Permissions.readComment:
                        if (item.CommentRead || item.CommentWrite) return true;
                        break;
                    case Permissions.writeComment:
                        if (item.CommentWrite) return true;
                        break;
                }
            }

            return false;
        }

        private async Task<bool> HasTagAccess(Guid boardId, Guid userId, Guid parentId)
        {
            var hasRole = await context.RoleSessions.AnyAsync(r => !r.Demoted && r.BoardId == boardId && r.UserId == userId);
            if (!hasRole) return true;

            var rolesTags = from roleSession in context.RoleSessions.Where(r => !r.Demoted && r.BoardId == boardId && r.UserId == userId)
                            join roleTags in context.RolesTagList on roleSession.RoleId equals roleTags.RoleId
                            select new
                            {
                                roleTags.Id,
                                roleTags.TagId,
                                roleTags.RoleId
                            };

            var taskTags = from tasktag in context.TagsList.Where(t => !t.Removed && t.TaskId == parentId)
                           select new
                           {
                               tasktag.Id,
                               tasktag.TagId
                           };

            if (!(await rolesTags.AnyAsync()) && !(await taskTags.AnyAsync())) return true;
            if (!(await taskTags.AnyAsync())) return true;

            foreach (var task in taskTags) //todo: what?? convert to linq
                foreach (var role in rolesTags)
                    if (task.TagId == role.TagId) return true;

            return false;
        }

    }
}
