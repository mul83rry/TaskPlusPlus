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

    enum Permissions { 
        readTask,
        writeTask,
        readComment,
        writeComment
    }

    public class TaskPlusPlusRepository : ITaskPlusPlusRepository, IDisposable
    {
        private TaskPlusPlusContext _context;

        public TaskPlusPlusRepository(TaskPlusPlusContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_context != null)
                {
                    _context.Dispose();
                    _context = null;
                }
            }
        }

        private async Task<Session> GetUserSessionAsync(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            var user = await _context.Sessions.SingleOrDefaultAsync(s => s.AccessToken == accessToken);
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return user;
        }

        public async Task<string> GetBoardsAsync(string accessToken)
        {
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();

            var res = from board in _context.Boards.Where(b => !b.Deleted)
                      join sharedBoard in _context.SharedBoards
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
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();

            var board = new Board()
            {
                Id = Guid.NewGuid(),
                CreationAt = DateTime.Now,
                Caption = caption,
                CreatorId = user.UserId,
                Deleted = false
            };
            await _context.Boards.AddAsync(board);

            // add to shareTo for this user
            var shareTo = new SharedBoard()
            {
                ShareTo = user.UserId,
                BoardId = board.Id
            };
            await _context.SharedBoards.AddAsync(shareTo);

            await _context.SaveChangesAsync();

            return new JObject { { "result", true } };
        }
        public async Task<JObject> UpdateBoardAsync(string accessToken, Guid boardId, string caption)
        {
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();

            // check accessibility
            if (!(await IsOwner(user.UserId,boardId)))
                return new JObject { { "result", false } };

            var board = await _context.Boards.SingleOrDefaultAsync(b => b.Id == boardId);
            board.Caption = caption;

            await _context.SaveChangesAsync();

            return new JObject { { "result", true } };
        }

        public async Task<JObject> DeleteBoardAsync(string accessToken, Guid boardId)
        {
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();

            var found = await _context.Boards.SingleOrDefaultAsync(b => b.Id == boardId);
            if (found != null)
            {
                // check accessibility
                if (!_context.Boards.Any(b => b.CreatorId == user.UserId))
                    return new JObject { { "result", false } };

                found.Deleted = true;
                await _context.SaveChangesAsync();
                return new JObject { { "result", true } };
            }
            return new JObject { { "result", false } };
        }

        public async Task<JObject> SigninAsync(string phoneNumber) // todo: edit need
        {
            if (!await _context.Users.AnyAsync(u => u.PhoneNumber == phoneNumber))
                return new JObject { { "result", false }, { "accessCode", string.Empty } };

            var user = await _context.Users.SingleOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
            if (user == null) return new JObject { { "result", false }, { "accessCode", string.Empty } };

            // save new session
            var newSession = new Session()
            {
                AccessToken = Guid.NewGuid().ToString(),
                Id = Guid.NewGuid(),
                IsValid = true,
                UserId = user.Id,
                CreationAt = DateTime.Now,
                LastFetchTime = DateTime.Now - TimeSpan.FromHours(1)
            };

            await _context.Sessions.AddAsync(newSession);
            await _context.SaveChangesAsync();
            return new JObject { { "result", true }, { "accessCode", newSession.AccessToken } };
        }
        public async Task<JObject> SignUpAsync(string firstName, string lastName, string phoneNumber)
        {
            if (await _context.Users.AnyAsync(u => u.PhoneNumber == phoneNumber))
                return new JObject { { "result", false }, { "accessCode", string.Empty } };

            var newUser = new User()
            {
                Id = Guid.NewGuid(),
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber,
                SignupDate = DateTime.Now
            };
            await _context.Users.AddAsync(newUser);

            // save new session
            var newSession = new Session()
            {
                AccessToken = Guid.NewGuid().ToString(),
                Id = Guid.NewGuid(),
                IsValid = true,
                UserId = newUser.Id,
                CreationAt = DateTime.Now
            };

            await _context.Sessions.AddAsync(newSession);
            await _context.SaveChangesAsync();
            return new JObject { { "result", true }, { "accessCode", newSession.AccessToken } };
        }

        public async Task<string> GetTasksAsync(string accessToken, Guid parentId)
        {
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();

            var boardId = await GetBoardIdAsync(parentId);

            var isOwner = await IsOwner(user.UserId, parentId);

            var jsonData = new JArray();

            if (!(await HaveAccessToTask(user.UserId,boardId))) return jsonData.ToString();

            if (!isOwner && !(await HasRoleAccess(Permissions.readTask, boardId, user.UserId))) return jsonData.ToString();

            

            var res = from task in _context.Tasks
                      .Where(t => t.ParentId == parentId && !t.Deleted).OrderBy(t => t.CreationAt)
                      select new
                      {
                          task.Id,
                          task.Caption,
                          task.Star,
                          task.CreationAt,
                          task.LastModifiedBy
                      };

            foreach (var item in res)
            {
                if (!isOwner && !(await HasTagAccess(boardId, user.UserId, item.Id)))
                    continue;

                jsonData.Add(new JObject
                    {
                        {"id", item.Id },
                        {"caption",  item.Caption },
                        {"star",  item.Star },
                        {"creationAt",  item.CreationAt },
                        {"haveChild", (await HaveChild(item.Id))["result"] },
                        { "lastModifiedBy", (await GetUser(item.LastModifiedBy)).FirstName.ToString()},
                        {"tags", (await GetTaskTagListAsync(item.Id))}
                    });
            }
            return jsonData.ToString();
        }

        public async Task<JObject> AddTaskAsync(string accessToken, Guid parentId, string caption)
        {
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();

            var board = await _context.Boards.SingleOrDefaultAsync(b => b.Id == parentId);

            var isOwner = await IsOwner(user.UserId, parentId);

            if (board == null) return new JObject { { "result", false } };

            // check accessibility
            if (!(await HaveAccessToTask(user.UserId, parentId))) 
                return new JObject { { "result", false } };

            if(!isOwner && !(await HasPermissions(user.UserId,parentId,Permissions.writeTask))) return new JObject { { "result", false } };

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

            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();

            return new JObject { { "result", true } };

        }


        public async Task<JObject> AddSubTaskAsync(string accessToken, Guid parentId, string caption)
        {
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();

            var isOwner = await IsOwner(user.UserId, parentId);

            // check accessibility
            if(!(await HaveAccessToTask(user.UserId,parentId))) return new JObject { { "result", false } };

            var task = await _context.Tasks.SingleOrDefaultAsync(t => t.Id == parentId);

            if (task == null) return new JObject { { "result", false } };

            if(!isOwner && !(await HasPermissions(user.UserId, parentId, Permissions.writeTask))) return new JObject { { "result", false } };



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

            await _context.Tasks.AddAsync(subTask);
            await _context.SaveChangesAsync();

            return new JObject { { "result", true } };
        }

        public async Task<JObject> EditTaskAsync(string accessToken, Guid parentId, string caption, bool star)
        {
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();

            var isOwner = await IsOwner(user.UserId, parentId);

            var task = await _context.Tasks.SingleOrDefaultAsync(t => t.Id == parentId && (t.Creator == user.UserId || isOwner));

            if (task == null) return new JObject { { "result", false } };

            // check accessibility
            if (!(await HaveAccessToTask(user.UserId, parentId)))
                return new JObject { { "result", false } };

            if (!isOwner && !(await HasPermissions(user.UserId, parentId, Permissions.writeTask))) 
                return new JObject { { "result", false } };

            task.Caption = caption;
            task.Star = star;
            task.LastModifiedBy = user.UserId; 

            await _context.SaveChangesAsync();

            return new JObject { { "result", true } };
        }

        public async Task<JObject> EditSubTaskAsync(string accessToken, Guid parentId, string caption, bool star)
        {
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();

            var isOwner = await IsOwner(user.UserId, parentId);

            var task = await _context.Tasks.SingleOrDefaultAsync(t => t.Id == parentId && (t.Creator == user.UserId || isOwner));

            if (task == null) 
                return new JObject { { "result", false } };

            if (!(await HaveAccessToTask(user.UserId,parentId))) 
                return new JObject { { "result", false } };

            if (!isOwner && !(await HasPermissions(user.UserId, parentId, Permissions.writeTask)))
                return new JObject { { "result", false } };


            task.Caption = caption;
            task.Star = star;
            task.LastModifiedBy = user.UserId;

            await _context.SaveChangesAsync();

            return new JObject { { "result", true } };
        }

        private async Task<bool> HaveAccessToTask(Guid userId, Guid parentId)
        {
            var pId = parentId;
            var tempTask = await _context.Tasks.SingleOrDefaultAsync(t => t.Id == pId); ;
            while (true)
            {
                tempTask = await _context.Tasks.SingleOrDefaultAsync(t => t.Id == pId);

                if (tempTask == null) break;
                pId = tempTask.ParentId;



            }

            var board = await _context.Boards.SingleOrDefaultAsync(b => b.Id == pId);
            return await _context.SharedBoards.AnyAsync(s => s.BoardId == board.Id && s.ShareTo == userId);
        }


        private async Task<bool> IsOwner(Guid userId, Guid parentId)
        {
            var pId = parentId;
            var tempTask = new Entities.Task();
            while (true)
            {
                tempTask = await _context.Tasks.SingleOrDefaultAsync(t => t.Id == pId);

                if (tempTask == null) break;
                pId = tempTask.ParentId;

            }

            var board = await _context.Boards.SingleOrDefaultAsync(b => b.Id == pId);
            return (userId == board.CreatorId);
        }




        public async Task<JObject> DeleteTaskAsync(string accessToken, Guid parentId)
        {
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();

            var isOwner = await IsOwner(user.UserId, parentId);

            var task = await _context.Tasks.SingleOrDefaultAsync(t => t.Id == parentId && (t.Creator == user.UserId || isOwner));

            if (task == null) 
                return new JObject { { "result", false } };

            if (!(await HaveAccessToTask(user.UserId, parentId))) 
                return new JObject { { "result", false } };


            if (!isOwner && !(await HasPermissions(user.UserId, parentId, Permissions.writeTask)))
                return new JObject { { "result", false } };

            task.Deleted = true;
            task.LastModifiedBy = user.UserId;


            await _context.SaveChangesAsync();

            return new JObject { { "result", true } };
        }


        private async Task<JObject> HaveChild(Guid taskId)
        {
            return new JObject { { "result", await _context.Tasks.AnyAsync(t => t.ParentId == taskId && t.Deleted == false) } };
        }


        public async Task<JObject> AddCommentAsync(string accessToken, Guid parentId, string text)
        {
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();

            var isOwner = await IsOwner(user.UserId, parentId);

            if (await HaveAccessToTask(user.UserId, parentId) == false) return new JObject { { "result", false } };

            if(!isOwner && !(await HasPermissions(user.UserId,parentId,Permissions.writeComment))) return new JObject { { "result", false } };


            var comment = new Entities.Comment()
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


            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            return new JObject { { "result", true } };
        }


        public async Task<string> GetCommentsAsync(string accessToken, Guid parentId)
        {
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();

            var isOwner = await IsOwner(user.UserId, parentId);

            if (await HaveAccessToTask(user.UserId, parentId) == false) return new JObject { { "result", false } }.ToString();

            if(!isOwner && !(await HasPermissions(user.UserId,parentId,Permissions.readComment))) return new JObject { { "result", false } }.ToString();


            var res = from comment in _context.Comments
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


        private async Task<User> GetUser(Guid userId)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId);


            return user;
        }


        public async Task<JObject> EditCommentAsync(string accessToken, Guid parentId, Guid commentId, string text)
        {
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();

            var isOwner = await IsOwner(user.UserId, parentId);

            if (await HaveAccessToTask(user.UserId, parentId) == false) return new JObject { { "result", false } };

            //find comment => create new comment => change edit id value to new comment id => save data base

            var oldComment = await _context.Comments.SingleOrDefaultAsync(c => c.Id == commentId && (c.Sender == user.UserId || isOwner));

            if(oldComment == null) return new JObject { { "result", false } };


            var comment = new Entities.Comment()
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

            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            return new JObject { { "result", true } };
        }

        public async Task<JObject> DeleteCommentAsync(string accessToken, Guid parentId, Guid commentId)
        {
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();

            var isOwner = await IsOwner(user.UserId, parentId);

            if (await HaveAccessToTask(user.UserId, parentId) == false) return new JObject { { "result", false } };

            var comment = await _context.Comments.SingleOrDefaultAsync(c => c.Id == commentId && (c.Sender == user.UserId || isOwner));

            if(comment == null) return new JObject { { "result", false } };

            comment.Deleted = true;
            comment.LastModifiedBy = user.UserId;

            await _context.SaveChangesAsync();

            return new JObject { { "result", true } };
        }


        public async Task<JObject> AddFriendAsync(string accessToken, string phoneNumber)
        {
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();

            var friendUser = await _context.Users.SingleOrDefaultAsync(u => u.PhoneNumber == phoneNumber);

            if (friendUser == null) return new JObject { { "result", false } };

            if (friendUser.Id == user.UserId) return new JObject { { "result", false } };

            // if already there is an active request between these two return false
            if (await _context.FriendLists.AnyAsync(f => (f.From == user.UserId && f.To == friendUser.Id && (!f.Removed && f.Accepted || f.Pending)) || (f.To == user.UserId && f.From == friendUser.Id && (!f.Removed && f.Accepted || f.Pending)))) 
                return new JObject { { "result", false } };

            var friend = new Entities.FriendList()
            {
                Id = Guid.NewGuid(),
                From = user.UserId,
                To = friendUser.Id,
                Pending = true,
                Accepted = false,
                RequestDate = DateTime.Now,
                Removed = false
            };


            await _context.FriendLists.AddAsync(friend);
            await _context.SaveChangesAsync();

            return new JObject { { "result", true } };
        }


        public async Task<string> GetFriendsListAsync(string accessToken)
        {
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();

            var res = from FList in _context.FriendLists.Where(f => (user.UserId == f.From || user.UserId == f.To) && !f.Pending && f.Accepted && !f.Removed).OrderBy(f => f.RequestDate)
                      select new
                      {
                          FList.From,
                          FList.To,
                          FList.Id
                      };

            var jsonData = new JArray();

            foreach (var item in res)
            {
                User userDetail = null;

                if (item.From == user.UserId)
                {
                    userDetail = await GetUser(item.To);
                }

                if (item.To == user.UserId)
                {
                    userDetail = await GetUser(item.From);
                }


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
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();

            var res = from FList in _context.FriendLists.Where(f => user.UserId == f.To && f.Pending).OrderBy(f => f.RequestDate)
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
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();

            var request = await _context.FriendLists.SingleOrDefaultAsync(f => f.Id == requestId);

            if (user.UserId != request.To) return new JObject { { "result", false } };

            if (!request.Pending) return new JObject { { "result", false } };





            request.Pending = false;
            request.Accepted = reply;

            await _context.SaveChangesAsync();

            return new JObject { { "result", true } };
        }


        public async Task<JObject> RemoveFriendAsync(string accessToken, Guid requestId)
        {
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();

            var request = await _context.FriendLists.SingleOrDefaultAsync(f => f.Id == requestId);

            if (request.From != user.UserId && request.To != user.UserId) return new JObject { { "result", false } };


            request.Removed = true;

            await _context.SaveChangesAsync();

            return new JObject { { "result", true } };
        }

        public async Task<JObject> ShareBoardAsync(string accessToken, Guid boardId, string shareToList)
        {
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();

            var board = await _context.Boards.SingleOrDefaultAsync(b => b.Id == boardId && b.CreatorId == user.UserId);

            

            if (board == null) return new JObject { { "result", false } };


            var shareToArray = shareToList.Split(',');

            foreach (var item in shareToArray)
            {
                if (item != "")
                {
                    var friend = await _context.Users.SingleOrDefaultAsync(u => u.PhoneNumber == item);
                    var shareTo = friend.Id;

                    if (!(await _context.FriendLists.AnyAsync(f => (f.From == user.UserId && f.To == shareTo && !f.Pending && !f.Removed && f.Accepted) || (f.To == user.UserId && f.From == shareTo && !f.Pending && !f.Removed && f.Accepted))))
                        return new JObject { { "result", false } };

                    if (await _context.SharedBoards.AnyAsync(s => s.BoardId == boardId && s.ShareTo == shareTo)) continue;


                        var share = new Entities.SharedBoard()
                    {
                        Id = Guid.NewGuid(),
                        BoardId = boardId,
                        ShareTo = shareTo,
                        GrantedAccessAt = DateTime.Now,
                    };

                    await _context.SharedBoards.AddAsync(share);
                }
            }

            await _context.SaveChangesAsync();


            return new JObject { { "result", true } };
        }


        public async Task<JObject> AddTagAsync(string accessToken, Guid boardId, string caption)
        {
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();

            var board = await _context.Boards.SingleOrDefaultAsync(b => b.Id == boardId && b.CreatorId == user.UserId);

            if (board == null) return new JObject { { "result", false } };


            var tag = new Entities.Tag()
            {
                Id = Guid.NewGuid(),
                BoardId = boardId,
                Caption = caption,
                Removed = false,
                CreationDate = DateTime.Now
            };

            await _context.Tags.AddAsync(tag);

            await _context.SaveChangesAsync();


            return new JObject { { "result", true } };
        }

        public async Task<string> GetTagListAsync(string accessToken, Guid parentId)
        {
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();

            var boardId = await GetBoardIdAsync(parentId);

            var res = from tag in _context.Tags.OrderBy(t => t.CreationDate)
                      join sharedBoard in _context.SharedBoards
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
            var user = await GetUserSessionAsync(acessToken) ?? throw new NullReferenceException();

            if (!(await IsOwner(user.UserId, taskId))) return new JObject { { "result", false } };

            if (!(await _context.Tags.AnyAsync(t => t.Id == tagId && !t.Removed))) return new JObject { { "result", false } };

            if(await _context.TagsList.AnyAsync(t => !t.Removed && t.TagId == tagId && t.TaskId == taskId)) return new JObject { { "result", false } };


            var taskTag = new Entities.TagsList()
            {
                Id = Guid.NewGuid(),
                TagId = tagId,
                TaskId = taskId,
                Removed = false,
                AsignDate = DateTime.Now
            };

            await _context.TagsList.AddAsync(taskTag);

            await _context.SaveChangesAsync();

            return new JObject { { "result", true } };
        }

        private async Task<JArray> GetTaskTagListAsync(Guid taskId)
        {


            var res = from tagList in _context.TagsList.Where(t => t.TaskId == taskId).OrderBy(t => t.AsignDate)
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
                        {"caption",  (await _context.Tags.SingleOrDefaultAsync(t => t.Id == item.TagId)).Caption }
                    });
                }

            }
            return jsonData;
        }

        public async Task<JObject> RemoveTagFromTaskAsync(string accessToken, Guid taskId , Guid taskTagId)
        {
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();

            if (!(await IsOwner(user.UserId, taskId))) return new JObject { { "result", false } };

            var taskTag = await _context.TagsList.SingleOrDefaultAsync(t => t.Id == taskTagId && !t.Removed);

            if (taskTag == null) return new JObject { { "result", false } };

            taskTag.Removed = true;

            await _context.SaveChangesAsync();


            return new JObject { { "result", true } };
        }

        public async Task<JObject> AddRoleAsync(string accessToken, Guid boardId, string caption, bool readTask, bool writeTask, bool readComment, bool writeComment, string tagList)
        {
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();

            if (!(await _context.Boards.AnyAsync(b => b.Id == boardId && b.CreatorId == user.UserId))) return new JObject { { "result", false } };

            var role = new Entities.Roles()
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

            await _context.Roles.AddAsync(role);

            await _context.SaveChangesAsync();

            if (tagList == "none") return new JObject { { "result", true } };

            var tagListArray = tagList.Split(',');

            foreach (var item in tagListArray)
            {
                if (item != string.Empty)
                {
                    if (!(await _context.Tags.AnyAsync(t => !t.Removed && t.Id == Guid.Parse(item) && t.BoardId == boardId))) continue;

                    var roleTag = new Entities.RolesTagList()
                    {
                        Id = Guid.NewGuid(),
                        RoleId = role.Id,
                        TagId = Guid.Parse(item)
                    };

                    await _context.RolesTagList.AddAsync(roleTag);
                }
            }

            await _context.SaveChangesAsync();

            return new JObject { { "result", true } };
        }

        public async Task<string> GetBoardRolesAsync(string accessToken, Guid boardId)
        {
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();

            var jsonData = new JArray();

            if (!(await _context.SharedBoards.AnyAsync(s => s.ShareTo == user.UserId && s.BoardId == boardId))) return jsonData.ToString();

            var res = from roles in _context.Roles.Where(r => r.BoardId == boardId && !r.Removed).OrderBy(s => s.CreatedAt)
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
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();

            var isOwner = await IsOwner(user.UserId, boardId);

            var selfPromote = await IsOwner(employeesId, boardId);

            var isShared = await _context.SharedBoards.AnyAsync(s => s.BoardId == boardId && s.ShareTo == employeesId);

            var roleExist = await _context.Roles.AnyAsync(r => r.Id == roleId && !r.Removed);

            if (!isOwner || !isShared || !roleExist || selfPromote) return new JObject { { "result", false } };


            var employeesRole = new Entities.RoleSession()
            {
                Id = Guid.NewGuid(),
                UserId = employeesId,
                RoleId = roleId,
                BoardId = boardId,
                Demoted = false,
                AsignDate = DateTime.Now
            };

            await _context.RoleSessions.AddAsync(employeesRole);

            await _context.SaveChangesAsync();


            return new JObject { { "result", true } };
        }

        public async Task<JObject> RemoveTagAsync(string accessToken, Guid boardId, Guid tagId)
        {
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();

            var isOwner = await IsOwner(user.UserId, boardId);

            var isUsing = await TagIsUsing(tagId);

            if (!isOwner || isUsing) return new JObject { { "result", false } };

            var tag = await _context.Tags.SingleOrDefaultAsync(t => t.Id == tagId);

            tag.Removed = true;

            await _context.SaveChangesAsync();

            return new JObject { { "result", true } };
        }


        public async Task<bool> TagIsUsing(Guid tagId)
        {
            //fix check if task deleted
            var tagIsUsingInTask = false;
            var tagIsUsingInRole = false;



            var tagsInTasks = from tasksTag in _context.TagsList.Where(t => t.TagId == tagId)
                              join task in _context.Tasks.Where(t => !t.Deleted) on tasksTag.TaskId equals task.Id
                              select new
                              {
                                  tasksTag.Id,
                              };


            var tagsInRoles = from rolesTag in _context.RolesTagList.Where(r => r.TagId == tagId) join role in _context.Roles.Where(r => !r.Removed) on rolesTag.RoleId equals role.Id
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
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();


            if (!(await IsOwner(user.UserId, boardId))) return new JObject { { "result", false } };


            if (await _context.RoleSessions.AnyAsync(r => r.RoleId == roleId && !r.Demoted)) return new JObject { { "result", false } };


            var role = await _context.Roles.SingleOrDefaultAsync(r => r.Id == roleId);

            role.Removed = true;

            await _context.SaveChangesAsync();


            return new JObject { { "result", true } };
        }

        public async Task<JObject> DemoteEmployeesAsync(string accessToken, Guid boardId, Guid roleSessionId)
        {
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();

            if (!(await IsOwner(user.UserId, boardId))) return new JObject { { "result", false } };

            var roleSession = await _context.RoleSessions.SingleOrDefaultAsync(r => r.Id == roleSessionId && !r.Demoted);

            if (roleSession == null) return new JObject { { "result", false } };

            

            roleSession.Demoted = true;

            await _context.SaveChangesAsync();

            return new JObject { { "result", true } };
        }

        public async Task<string> GetEmployeesRolesAsync(string accessToken, Guid boardId)
        {
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();

            var jsonData = new JArray();

            if (!(await _context.SharedBoards.AnyAsync(s => s.BoardId == boardId && s.ShareTo == user.UserId))) return jsonData.ToString();

            var roleSessions = from roleSession in _context.RoleSessions.Where(r => r.BoardId == boardId && !r.Demoted).OrderBy(r => r.AsignDate)
                               select new
                               {
                                   roleSession.Id,
                                   roleSession.UserId,
                                   roleSession.RoleId,
                                   roleSession.AsignDate
                               };

            foreach(var item in roleSessions)
            {
                var employe = await GetUser(item.UserId);
                var role = await _context.Roles.SingleOrDefaultAsync(r => r.Id == item.RoleId);


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
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();

            var board = await _context.Boards.SingleOrDefaultAsync(b => b.Id == boardId);

            var isShared = await _context.SharedBoards.AnyAsync(s => s.BoardId == boardId && s.ShareTo == user.UserId);

            var jsonData = new JArray();

            if (board == null || !isShared) return jsonData.ToString();


            var res = from sharedBoard in _context.SharedBoards.Where(s => s.BoardId == boardId && s.ShareTo != board.CreatorId).OrderBy(s => s.GrantedAccessAt)
                      select new
                      {
                          sharedBoard.ShareTo,
                      };

            foreach(var item in res)
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


        private async Task<bool> HasPermissions(Guid userId,Guid parentId,Permissions permissionType)
        {
            var boardId = await GetBoardIdAsync(parentId);

            var RoleAccess = await HasRoleAccess(permissionType, boardId, userId);

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
                tempTask = await _context.Tasks.SingleOrDefaultAsync(t => t.Id == boardId);

                if (tempTask == null) break;
                boardId = tempTask.ParentId;
            }

            return boardId;
        }

        private async Task<bool> HasRoleAccess(Permissions permissionType, Guid boardId, Guid userId)
        {
            var roles = from roleSession in _context.RoleSessions.Where(r => !r.Demoted && r.BoardId == boardId && r.UserId == userId)
                        join role in _context.Roles on roleSession.RoleId equals role.Id
                        select new
                        {
                            role.Id,
                            role.TaskRead,
                            role.TaskWrite,
                            role.CommentRead,
                            role.CommentWrite
                        };

            if (!(await roles.AnyAsync())) return true;


            foreach(var item in roles)
            {
                switch(permissionType)
                {
                    case Permissions.readTask:
                        if (item.TaskRead || item.TaskWrite) return true;
                        break;
                    case Permissions.writeTask:
                        if(item.TaskWrite) return true;
                        break;
                    case Permissions.readComment:
                        if(item.CommentRead || item.CommentWrite) return true;
                        break;
                    case Permissions.writeComment:
                        if(item.CommentWrite) return true;
                        break;
                }
            }

            return false;
        }

        private async Task<bool> HasTagAccess(Guid boardId, Guid userId, Guid parentId)
        {
           var hasRole = await _context.RoleSessions.AnyAsync(r => !r.Demoted && r.BoardId == boardId && r.UserId == userId);
           
           if(!hasRole) return true;

            var rolesTags = from roleSession in _context.RoleSessions.Where(r => !r.Demoted && r.BoardId == boardId && r.UserId == userId)
                                       join roleTags in _context.RolesTagList on roleSession.RoleId equals roleTags.RoleId
                                       select new
                                       {
                                           roleTags.Id,
                                           roleTags.TagId,
                                           roleTags.RoleId
                                       };
            


            var taskTags = from tasktag in _context.TagsList.Where(t => !t.Removed && t.TaskId == parentId)
                           select new
                           {
                               tasktag.Id,
                               tasktag.TagId
                           };

            if (!(await rolesTags.AnyAsync()) && !(await taskTags.AnyAsync())) return true;

            if (!(await taskTags.AnyAsync())) return true;


            foreach(var task in taskTags)
            {
                foreach(var role in rolesTags)
                {
                    if (task.TagId == role.TagId)
                        return true;
                }
            }


            return false;
        }
    }


  
}
