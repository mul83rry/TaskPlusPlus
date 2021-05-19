﻿using TaskPlusPlus.API.DbContexts;
using TaskPlusPlus.API.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

        private async Task<Session> GetUserAsync(string accessToken)
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
            var user = await GetUserAsync(accessToken) ?? throw new NullReferenceException();

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
                    {"CreatorId",  item.CreatorId },
                    {"caption",  item.Caption },
                    {"creationAt",  item.CreationAt }
                });
            }
            return jsonData.ToString();
        }

        public async Task<bool> AddBoardAsync(string accessToken, string caption)
        {
            var user = await GetUserAsync(accessToken) ?? throw new NullReferenceException();

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
                ShareTo = user.Id
            };
            await _context.SharedBoards.AddAsync(shareTo);

            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<bool> UpdateBoardAsync(string accessToken, Guid boardId, string caption)
        {
            var user = await GetUserAsync(accessToken) ?? throw new NullReferenceException();

            // check accessibility
            if (!_context.SharedBoards.Any(b => b.ShareTo == user.Id && b.BoardId == boardId))
                return false;

            var board = await _context.Boards.SingleOrDefaultAsync(b => b.Id == boardId);
            board.Caption = caption;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteBoardAsync(string accessToken, Guid boardId)
        {
            var user = await GetUserAsync(accessToken) ?? throw new NullReferenceException();

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
        public async Task<JObject> SignupAsync(string firstName, string lastName, string phoneNumber)
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
            var user = await GetUserAsync(accessToken) ?? throw new NullReferenceException();

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

        public async Task<bool> AddTaskAsync(string accessToken, Guid parentId, string caption)
        {
            var user = await GetUserAsync(accessToken) ?? throw new NullReferenceException();

            var board = await _context.Boards.SingleOrDefaultAsync(b => b.Id == parentId);
            if (board == null) return false;

            // check accessibility
            if (!_context.SharedBoards.Any(b => b.ShareTo == user.Id && b.BoardId == board.Id))
                return false;

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

            return true;
        }

        private async Task<bool> HaveAccessToSubTaskAsync(Guid parentId)
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

        public async Task<bool> AddSubTaskAsync(string accessToken, Guid parentId, string caption)
        {
            var user = await GetUserAsync(accessToken) ?? throw new NullReferenceException();

            var task = await _context.Tasks.SingleOrDefaultAsync(t => t.Id == parentId);
            if (task == null) return false;

            if (await HaveAccessToSubTaskAsync(parentId) == false) return false;

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

            return true;
        }

        public async Task<bool> EditTaskAsync(string accessToken, Guid parentId, string caption, bool star)
        {
            var user = await GetUserAsync(accessToken) ?? throw new NullReferenceException();

            var task = await _context.Tasks.SingleOrDefaultAsync(t => t.Id == parentId);
            if (task == null) return false;

            // check accessibility
            if (!_context.SharedBoards.Any(b => b.ShareTo == user.Id && task.Id == parentId))
                return false;

            task.Caption = caption;
            task.Star = star;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> EditSubTaskAsync(string accessToken, Guid parentId, string caption, bool star)
        {
            var user = await GetUserAsync(accessToken) ?? throw new NullReferenceException();

            var task = await _context.Tasks.SingleOrDefaultAsync(t => t.Id == parentId);
            if (task == null) return false;

            // check accessibility
            if (await HaveAccessToSubTaskAsync(parentId) == false) return false;

            task.Caption = caption;
            task.Star = star;

            await _context.SaveChangesAsync();

            return true;
        }

    }
}