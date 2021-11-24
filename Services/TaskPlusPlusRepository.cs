using TaskPlusPlus.API.DbContexts;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TaskPlusPlus.API.Services
{
    public partial class TaskPlusPlusRepository : ITaskPlusPlusRepository, IDisposable
    {
        private TaskPlusPlusContext context;

        public TaskPlusPlusRepository(TaskPlusPlusContext context) => this.context = context ?? throw new ArgumentNullException(nameof(context));


        public async Task AddFakeData()
        {
            int length = 1000;
            for (int i = 0; i < length; i++)
            {
                var newSession = new Entities.Session()
                {
                    AccessToken = Guid.NewGuid().ToString(),
                    Id = Guid.NewGuid(),
                    IsValid = true,
                    UserId = Guid.NewGuid(),
                    OsVersion = "osVersion",
                    DeviceType = "deviceType",
                    BrowerVersion = "browerVersion",
                    Orientation = "orientation",
                    CreationAt = DateTime.Now
                };
                await context.Sessions.AddAsync(newSession);
            }

            for (int i = 0; i < length; i++)
            {
                var newComment = new Entities.Comment()
                {
                    Id = Guid.NewGuid(),
                    Text = "content",
                    Sender = Guid.NewGuid(),
                    ReplyTo = Guid.NewGuid(),
                    ParentId = Guid.NewGuid(),
                    CreationDate = DateTime.Now,
                    Deleted = false,
                    EditId = Guid.NewGuid(),
                    LastModifiedBy = Guid.NewGuid()
                };
                context.Comments.Add(newComment);
            }

            for (int i = 0; i < length; i++)
            {
                var newTask = new Entities.Task()
                {
                    Id = Guid.NewGuid(),
                    Caption = "caption",
                    ParentId = Guid.NewGuid(),
                    Star = false,
                    CreationAt = DateTime.Now,
                    Deleted = false,
                    Creator = Guid.NewGuid(),
                    LastModifiedBy = Guid.NewGuid(),
                    Compeleted = false,
                    Status = "ToDo",
                };

                await context.Tasks.AddAsync(newTask);
            }

            for (int i = 0; i < length; i++)
            {
                var newBoard = new Entities.Board()
                {
                    Id = Guid.NewGuid(),
                    CreationAt = DateTime.Now,
                    Caption = "caption",
                    CreatorId = Guid.NewGuid(),
                    Deleted = false
                };

                await context.Boards.AddAsync(newBoard);
            }

            for (int i = 0; i < length; i++)
            {
                var newShareTo = new Entities.SharedBoard()
                {
                    ShareTo = Guid.NewGuid(),
                    BoardId = Guid.NewGuid()
                };

                await context.SharedBoards.AddAsync(newShareTo);
            }


            for (int i = 0; i < length; i++)
            {
                var tag = new Entities.Tag()
                {
                    Id = Guid.NewGuid(),
                    BoardId = Guid.NewGuid(),
                    Caption = "caption",
                    Deleted = false,
                    CreationDate = DateTime.Now,
                    BackgroundColor = "#a244fa",
                };
                await context.Tags.AddAsync(tag);
            }

            for (int i = 0; i < length; i++)
            {
                var taskTag = new Entities.TagsList()
                {
                    Id = Guid.NewGuid(),
                    TagId = Guid.NewGuid(),
                    TaskId = Guid.NewGuid(),
                    Deleted = false,
                    AsignDate = DateTime.Now
                };

                await context.TagsList.AddAsync(taskTag);
            }

            for (int i = 0; i < length; i++)
            {
                var friend = new Entities.FriendList()
                {
                    Id = Guid.NewGuid(),
                    From = Guid.NewGuid(),
                    To = Guid.NewGuid(),
                    Pending = true,
                    Accepted = false,
                    RequestDate = DateTime.Now,
                    Removed = false,
                    ApplyDate = DateTime.Now,
                };

                await context.FriendLists.AddAsync(friend);
            }

            for (int i = 0; i < length; i++)
            {
                var roleTag = new Entities.RolesTagList()
                {
                    Id = Guid.NewGuid(),
                    TagId = Guid.NewGuid(),
                    RoleId = Guid.NewGuid(),
                    Removed = false,
                    AsignDate = DateTime.Now
                };

                await context.RolesTagList.AddAsync(roleTag);
            }

            for (int i = 0; i < length; i++)
            {
                var role = new Entities.Roles()
                {
                    Id = Guid.NewGuid(),
                    BoardId = Guid.NewGuid(),
                    Caption = "caption",
                    TaskRead = true,
                    TaskWrite = true,
                    TaskCompelete = true,
                    CommentRead = true,
                    CommentWrite = true,
                    Deleted = false,
                    CreatedAt = DateTime.Now,
                    BackgroundColor = "color",
                };

                await context.Roles.AddAsync(role);
            }

            for (int i = 0; i < length; i++)
            {
                var employeesRole = new Entities.RoleSession()
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    RoleId = Guid.NewGuid(),
                    BoardId = Guid.NewGuid(),
                    Demoted = false,
                    ShareSession = Guid.NewGuid(),
                    AsignDate = DateTime.Now
                };

                await context.RoleSessions.AddAsync(employeesRole);
            }


            for (int i = 0; i < length; i++)
            {
                var newUser = new Entities.Login()
                {
                    Id = Guid.NewGuid(),
                    PhoneNumber = "phoneNumber",
                    Email = string.Empty
                };

                await context.Login.AddAsync(newUser);
            }

            for (int i = 0; i < length; i++)
            {
                var newProfile = new Entities.Profile()
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    Bio = string.Empty,
                    Image = string.Empty,
                    PhoneNumber = "PhoneNumber",
                    SignupDate = DateTime.Now,
                    FirstName = "User",
                    LastName = string.Empty
                };

                await context.Profiles.AddAsync(newProfile);
            }


            await context.SaveChangesAsync();
        }



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


        private async Task<bool> HasPermissions(Guid userId, Guid parentId, Permissions permissionType)
        {
            var boardId = await GetBoardIdAsync(parentId);
            var RoleAccess = await HasRoleAccess(boardId, userId, permissionType);
            var tagAccess = await HasTagAccess(boardId, userId, parentId);

            if (!RoleAccess || !tagAccess) return false;

            return true;
        }

        public async Task<string> GetRecentChangesAsync(string accessToken)
        {
            var user = await GetUserSessionAsync(accessToken);

            var pendingFriendRequests = from friends in context.FriendLists.Where(f => f.Pending && f.To == user.UserId)
                                        select new
                                        {
                                            friends.Id,
                                        };

            var newMessages = from msg in context.Messages.Where(m => !m.Seen && m.UserId == user.UserId)
                              select new
                              {
                                  msg.Id,
                              };

            var JsonData = new JObject
            {
                new JProperty("NewMessages",newMessages.Count().ToString()),
                new JProperty("NewFriends", pendingFriendRequests.Count().ToString())
            };

            return JsonData.ToString();
        }

    }


}
