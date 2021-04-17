using TaskPlusPlus.API.DbContexts;
using TaskPlusPlus.API.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskPlusPlus.API.Services
{
    public class CourseLibraryRepository : ICourseLibraryRepository, IDisposable
    {
        private readonly TaskPlusPlusContext _context;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose resources when needed
            }
        }

        private async Task<Session> GetUser(string accessToken)
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

        public async Task<string> GetBoards(string accessToken)
        {
            var user = await GetUser(accessToken);

            var res = from board in _context.Boards
                      join sharedBoard in _context.SharedBoards
                      .Where(s => s.ShareTo == user.UserId).OrderBy(s => s.GrantedAccessAt)
                      on board.Id equals sharedBoard.ShareTo
                      select new
                      {
                          board.Id,
                          board.CreatorId,
                          board.Caption,
                          board.CreationAt
                      };

            var jsonData = new JArray();
            foreach (var item in res)
            {
                jsonData.Add(new JObject
                {
                    {"id", item.Id },
                    {"creaotrId",  item.CreatorId },
                    {"caption",  item.Caption },
                    {"creationAt",  item.CreationAt }
                });
            }
            return jsonData.ToString();
        }

        public async Task<bool> AddBoard(string accessToken, string caption)
        {
            var user = await GetUser(accessToken);

            var board = new Board()
            {
                Caption = caption,
                CreatorId = user.UserId
            };
            await _context.Boards.AddAsync(board);

            // add to shareTo for this user
            var shareTo = new SharedBoard()
            {
                ShareTo = user.Id
            };
            await _context.SharedBoards.AddAsync(shareTo);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateBoard(string accessToken, Guid boardId)
        {
            var user = await GetUser(accessToken);

            var found = await _context.Boards.SingleOrDefaultAsync(b => b.Id == boardId);
            if (found != null)
            {
                // check accessibility
                if (!_context.SharedBoards.Any(b => b.ShareTo == user.Id && b.BoardId == boardId))
                    return false;

                found.Caption = found.Caption;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteBoard(string accessToken, Guid boardId)
        {
            var user = await GetUser(accessToken);

            var found = await _context.Boards.SingleOrDefaultAsync(b => b.Id == boardId);
            if (found != null)
            {
                // check accessibility
                if (!_context.Boards.Any(b => b.CreatorId == user.Id))
                    return false;

                found.Deleted = true;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<Tuple<bool, string>> Signin(string phoneNumber) // todo: edit need
        {
            if (! await _context.Users.AnyAsync(u => u.PhoneNumber == phoneNumber))
                return new Tuple<bool, string>(false, string.Empty);

            var user = await _context.Users.SingleOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
            if (user == null) return new Tuple<bool, string>(false, string.Empty);
            
            // save new session
            var session = new Session()
            {
                IsValid = true,
                UserId = user.Id
            };

            return new Tuple<bool, string>(true, session.AccessToken);
        }
        public async Task<Tuple<bool, string>> Signup(string firstName, string lastName, string phoneNumber)
        {
            if (! await _context.Users.AnyAsync(u => u.PhoneNumber == phoneNumber))
                return new Tuple<bool, string>(false, string.Empty);

            var newUser = new User()
            {
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber
            };
            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            // save new session
            var session = new Session()
            {
                IsValid = true,
                UserId = newUser.Id
            };

            return new Tuple<bool, string>(true, session.AccessToken);
        }

        public async Task<string> GetTasks(string accessToken, Guid parentId)
        {
            var user = await GetUser(accessToken);

            var res = from task in _context.Tasks
                      .Where(t => t.ParentId == parentId && !t.Deleted).OrderBy(t => t.CreationAt)
                      select new
                      {
                          task.Id,
                          task.Caption,
                          task.Star,
                          task.CreationAt
                      };

            var jsonData = new JArray();
            foreach (var item in res)
            {
                jsonData.Add(new JObject
                {
                    {"id", item.Id },
                    {"caption",  item.Caption },
                    {"star",  item.Star },
                    {"creationAt",  item.CreationAt }
                });
            }
            return jsonData.ToString();
        }

        public async Task<bool> AddTask(string accessToken, Guid parentId, string caption)
        {
            var user = await GetUser(accessToken);

            var board = await _context.Boards.SingleOrDefaultAsync(b => b.Id == parentId);
            if (board == null) return false;

            // check accessibility
            if (!_context.SharedBoards.Any(b => b.ShareTo == user.Id && b.BoardId == board.Id))
                return false;

            var task = new Entities.Task()
            {
                Caption = caption,
                ParentId = board.Id
            };

            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();

            return true;
        }

        private async Task<bool> HaveAccessToSubTask(Guid parentId)
        {
            // todo: find first subtask, find task, check accessibility
            var pId = parentId;

            while (true)
            {
                var temptask = await _context.Tasks.SingleOrDefaultAsync(t => t.ParentId == pId);
                var board = await _context.Boards.SingleOrDefaultAsync(b => b.Id == temptask.Id);
                if (board == null) continue;

                pId = board.Id;
                break;
            }

            // check accessibility
            if (!_context.SharedBoards.Any(b => b.BoardId == pId))
                return false;

            return true;
        }

        public async Task<bool> AddSubTask(string accessToken, Guid parentId, string caption)
        {
            var user = await GetUser(accessToken);

            var task = await _context.Tasks.SingleOrDefaultAsync(t => t.Id == parentId);
            if (task == null) return false;

            if (await HaveAccessToSubTask(parentId) == false) return false;

            var subTask = new Entities.Task()
            {
                Caption = caption,
                ParentId = task.Id
            };

            await _context.Tasks.AddAsync(subTask);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> EditTask(string accessToken, Guid parentId, string caption)
        {
            var user = await GetUser(accessToken);

            var task = await _context.Tasks.SingleOrDefaultAsync(t => t.Id == parentId);
            if (task == null) return false;

            // check accessibility
            if (!_context.SharedBoards.Any(b => b.ShareTo == user.Id))
                return false;

            task.Caption = caption;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> EditSubTask(string accessToken, Guid parentId, string caption)
        {
            var user = await GetUser(accessToken);

            var task = await _context.Tasks.SingleOrDefaultAsync(t => t.Id == parentId);
            if (task == null) return false;

            // check accessibility
            if (await HaveAccessToSubTask(parentId) == false) return false;

            task.Caption = caption;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
