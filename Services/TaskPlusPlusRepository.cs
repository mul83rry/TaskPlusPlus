using TaskPlusPlus.API.DbContexts;
using TaskPlusPlus.API.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TaskPlusPlus.API.Services
{
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

            var res = from board in _context.Boards
                      join sharedBoard in _context.SharedBoards
                      .Where(shared => shared.ShareTo == user.UserId).OrderBy(s => s.GrantedAccessAt)
                      on board.Id equals sharedBoard.BoardId
                      select new
                      {
                          board.Id,
                          board.CreatorId,
                          board.Caption,
                          board.CreationAt,
                          board.Deleted
                      };

            var jsonData = new JArray();
            foreach (var item in res)
            {
                if(!item.Deleted)
                {
                    jsonData.Add(new JObject
                    {
                        {"id", item.Id },
                        {"CreatorId",  item.CreatorId },
                        {"caption",  item.Caption },
                        {"creationAt",  item.CreationAt }
                    });
                }
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
            if (!_context.SharedBoards.Any(b => b.ShareTo == user.UserId && b.BoardId == boardId))
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

            var res = from task in _context.Tasks
                      .Where(t => t.ParentId == parentId && !t.Deleted).OrderBy(t => t.CreationAt)
                      select new
                      {
                          task.Id,
                          task.Caption,
                          task.Star,
                          task.CreationAt,
             
                      };

            var jsonData = new JArray();
            foreach (var item in res)
            {
                    jsonData.Add(new JObject
                    {
                        {"id", item.Id },
                        {"caption",  item.Caption },
                        {"star",  item.Star },
                        {"creationAt",  item.CreationAt },
                        { "haveChild", (await HaveChild(item.Id))["result"] },
                    });
            }
            return jsonData.ToString();
        }

        public async Task<JObject> AddTaskAsync(string accessToken, Guid parentId, string caption)
        {
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();

            var board = await _context.Boards.SingleOrDefaultAsync(b => b.Id == parentId);
            if (board == null) return new JObject { { "result", false } };

            // check accessibility
            if (!_context.SharedBoards.Any(b => b.ShareTo == user.UserId && b.BoardId == board.Id))
                return new JObject { { "result", false } };

            var task = new Entities.Task()
            {
                Id = Guid.NewGuid(),
                Caption = caption,
                ParentId = board.Id,
                Star = false,
                CreationAt = DateTime.Now,
                Deleted = false
            };

            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();

            return new JObject { { "result", true } };

        }

        private async Task<bool> HaveAccessToSubTaskAsync(Guid parentId, Guid userId)
        {
            // todo: find first sub task, find task, check accessibility
            var pId = parentId;

            while (true)
            {
                var tempTask = await _context.Tasks.SingleOrDefaultAsync(t => t.Id == pId);
                var board = await _context.Boards.SingleOrDefaultAsync(b => b.Id == tempTask.ParentId);
                if (board == null) continue;

                pId = board.Id;
                break;
            }

            // check accessibility
            return _context.SharedBoards.Any(b => b.BoardId == pId && b.ShareTo == userId); // todo: check
        }


        public async Task<JObject> AddSubTaskAsync(string accessToken, Guid parentId, string caption)
        {
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();

            // check accessibility
            //if (await HaveAccessToSubTaskAsync(parentId, user.UserId) == false) return new JObject { { "result", false } };

            var task = await _context.Tasks.SingleOrDefaultAsync(t => t.Id == parentId);
            if (task == null) return new JObject { { "result", false } };


            var subTask = new Entities.Task()
            {
                Id = Guid.NewGuid(),
                Caption = caption,
                ParentId = task.Id,
                Star = false,
                CreationAt = DateTime.Now,
                Deleted = false
            };

            await _context.Tasks.AddAsync(subTask);
            await _context.SaveChangesAsync();

            return new JObject { { "result", true } };
        }

        public async Task<JObject> EditTaskAsync(string accessToken, Guid parentId, string caption, bool star)
        {
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();

            var task = await _context.Tasks.SingleOrDefaultAsync(t => t.Id == parentId);
            if (task == null) return new JObject { { "result", false } };

            // check accessibility
            if (!_context.SharedBoards.Any(b => b.ShareTo == user.UserId && task.Id == parentId))
                return new JObject { { "result", false } };

            task.Caption = caption;
            task.Star = star;

            await _context.SaveChangesAsync();

            return new JObject { { "result", true } };
        }

        public async Task<JObject> EditSubTaskAsync(string accessToken, Guid parentId, string caption, bool star)
        {
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();

            // check accessibility
            //if (await HaveAccessToEditSubTaskAsync(parentId, user.UserId) == false) return new JObject { { "result", false } };

            var task = await _context.Tasks.SingleOrDefaultAsync(t => t.Id == parentId);
            if (task == null) return new JObject { { "result", false } };


            task.Caption = caption;
            task.Star = star;

            await _context.SaveChangesAsync();

            return new JObject { { "result", true } };
        }

        private async Task<bool> haveAccessToTask(Guid userId,Guid parentId)
        {
            var pId = parentId;
            var tempTask = await _context.Tasks.SingleOrDefaultAsync(t => t.Id == pId); ;
            while(true)
            {
                tempTask = await _context.Tasks.SingleOrDefaultAsync(t => t.Id == pId);

                if (tempTask == null) break;
                pId = tempTask.ParentId;

               

            }

                var board = await _context.Boards.SingleOrDefaultAsync(b => b.Id == pId);
                return await _context.SharedBoards.AnyAsync(s => s.BoardId == board.Id && s.ShareTo == userId);
        }




        public async Task<JObject> DeleteTaskAsync(string accessToken,Guid parentId)
        {
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();
            var task = await _context.Tasks.SingleOrDefaultAsync(t => t.Id == parentId);


            if(task == null) return new JObject { { "result", false } };
            //check access
            if (await haveAccessToTask(user.UserId, parentId) == false) return new JObject { { "result", false } };

            task.Deleted = true;
            await _context.SaveChangesAsync();

            return new JObject { { "result", true } };
        }


        private async Task<JObject> HaveChild(Guid taskId)
        {
            // check accessibility
            //return await HaveAccessToSubTaskAsync(taskId, user.UserId) == false ?
                //new JObject { { "result", false } } :
                return new JObject { { "result", await _context.Tasks.AnyAsync(t => t.ParentId == taskId && t.Deleted == false) } };
        }


        public async Task<JObject> AddCommentAsync(string accessToken,Guid parentId,string text)
        {
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();

            if (await haveAccessToTask(user.UserId, parentId) == false) return new JObject { { "result", false } };



            var comment = new Entities.Comment()
            {

                Id = Guid.NewGuid(),
                Text = text,
                Sender = user.UserId,
                ReplyTo = parentId,
                CreationDate = DateTime.Now,
                Deleted = false,
                EditId = "0",
            };


            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            return new JObject { { "result", true } };
        }


        public async Task<string> GetCommentsAsync(string accessToken,Guid parentId)
        {
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();


            if (await haveAccessToTask(user.UserId, parentId) == false) return new JObject { { "result", false } }.ToString();

            var res = from comment in _context.Comments
                      .Where(c => c.ReplyTo == parentId && c.Deleted == false && c.EditId == "0").OrderBy(c => c.CreationDate)
                      select new
                      {
                          comment.Id,
                          comment.Text,
                          comment.Sender,
                          comment.CreationDate,
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
                    });
            }
            return jsonData.ToString();
        }


        private async Task<User> GetUser(Guid userId)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId);


            return user;
        }


        public async Task<JObject> EditCommentAsync(string accessToken,Guid parentId, Guid commentId,string text)
        {
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();

            if (await haveAccessToTask(user.UserId, parentId) == false) return new JObject { { "result", false } };


            //find comment => create new comment => change edit id value tp new comment id => save data base

            var oldComment = await _context.Comments.SingleOrDefaultAsync(c => c.Id == commentId);

            var comment = new Entities.Comment()
            {
                Id = Guid.NewGuid(),
                Text = text,
                Sender = user.UserId,
                ReplyTo = parentId,
                CreationDate = oldComment.CreationDate,
                Deleted = false,
                EditId = "0",
            };

            oldComment.EditId = comment.Id.ToString();

            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            return new JObject { { "result", true } };
        }

        public async Task<JObject> DeleteCommentAsync(string accessToken,Guid parentId,Guid commentId)
        {
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();

            if (await haveAccessToTask(user.UserId, parentId) == false) return new JObject { { "result", false } };

            var comment = await _context.Comments.SingleOrDefaultAsync(c => c.Id == commentId);

            comment.Deleted = true;

            await _context.SaveChangesAsync();

            return new JObject { { "result", true } };
        }


        public async Task<JObject> AddFriendAsync(string accessToken, string phoneNumber)
        {
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();

            var friendUser = await _context.Users.SingleOrDefaultAsync(u => u.PhoneNumber == phoneNumber);

            if (friendUser == null) return new JObject { { "result", false } };

            if(friendUser.Id == user.UserId) return new JObject { { "result", false } };

            // if already there is an active request between these two return false
            if (await _context.FriendLists.AnyAsync(f => (f.From == user.UserId &&  f.To == friendUser.Id && (!f.Removed && f.Accepted || f.Pending)) || (f.To == user.UserId && f.From == friendUser.Id && (!f.Removed && f.Accepted || f.Pending)))) return new JObject { { "result", false } };

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

                if(item.From == user.UserId)
                {
                    userDetail = await GetUser(item.To);
                }

                if(item.To == user.UserId)
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

            if(!request.Pending) return new JObject { { "result", false } };





            request.Pending = false;
            request.Accepted = reply;

            await _context.SaveChangesAsync();

            return new JObject { { "result", true } };
        }


        public async Task<JObject> RemoveFriendAsync(string accessToken,Guid requestId)
        {
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();

            var request = await _context.FriendLists.SingleOrDefaultAsync(f => f.Id == requestId);

            if(request.From != user.UserId && request.To != user.UserId) return new JObject { { "result", false } };


            request.Removed = true;

            await _context.SaveChangesAsync();

            return new JObject { { "result", true } };
        }

        public async Task<JObject> ShareBoardAsync(string accessToken,Guid boardId,string shareToList)
        {
            var user = await GetUserSessionAsync(accessToken) ?? throw new NullReferenceException();

            var board = await _context.Boards.SingleOrDefaultAsync(b => b.Id == boardId);

            if (board == null) return new JObject { { "result", false } };

            if(user.UserId != board.CreatorId) return new JObject { { "result", false } };

            var shareToArray = shareToList.Split(',');

            foreach(var item in shareToArray)
            {
                if(item != "")
                {
                    var friend = await _context.Users.SingleOrDefaultAsync(u => u.PhoneNumber == item);
                    var shareTo = friend.Id;

                    if (!(await _context.FriendLists.AnyAsync(f => (f.From == user.UserId && f.To == shareTo && !f.Pending && !f.Removed && f.Accepted) || (f.To == user.UserId && f.From == shareTo && !f.Pending && !f.Removed && f.Accepted))))
                        return new JObject { { "result", false } };

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
    }
}
