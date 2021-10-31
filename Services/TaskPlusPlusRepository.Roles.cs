using TaskPlusPlus.API.Entities;
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
                Deleted = false,
                CreatedAt = DateTime.Now
            };

            await context.Roles.AddAsync(role);
            await context.SaveChangesAsync();

            if (tagList == "none") return JsonMap.TrueResult;

            var tagListArray = tagList.Split(',');

            foreach (var item in tagListArray)
            {
                if (string.IsNullOrEmpty(item)) continue;
                if (!await context.Tags.AnyAsync(t => !t.Deleted && t.Id == Guid.Parse(item) && t.BoardId == boardId)) continue;

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

            var res = from roles in context.Roles.Where(r => r.BoardId == boardId && !r.Deleted).OrderBy(s => s.CreatedAt)
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
            var isOwner = await IsOwnerOfBoard(user.UserId, boardId);
            var selfPromote = await IsOwnerOfBoard(employeesId, boardId);
            var isShared = await context.SharedBoards.AnyAsync(s => s.BoardId == boardId && s.ShareTo == employeesId);
            var roleExist = await context.Roles.AnyAsync(r => r.Id == roleId && !r.Deleted);

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



        public async Task<JObject> RemoveRoleFromBoardAsync(string accessToken, Guid boardId, Guid roleId)
        {
            var user = await GetUserSessionAsync(accessToken);
            if (!(await IsOwnerOfBoard(user.UserId, boardId))) return JsonMap.FalseResult;
            if (await context.RoleSessions.AnyAsync(r => r.RoleId == roleId && !r.Demoted)) return JsonMap.FalseResult;

            var role = await context.Roles.SingleOrDefaultAsync(r => r.Id == roleId);
            role.Deleted = true;
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<JObject> DemoteEmployeesRoleAsync(string accessToken, Guid boardId, Guid roleSessionId)
        {
            var user = await GetUserSessionAsync(accessToken);
            if (!(await IsOwnerOfBoard(user.UserId, boardId))) return JsonMap.FalseResult;

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
                    case Permissions.ReadTask:
                        if (item.TaskRead || item.TaskWrite) return true;
                        break;
                    case Permissions.WriteTask:
                        if (item.TaskWrite) return true;
                        break;
                    case Permissions.ReadComment:
                        if (item.CommentRead || item.CommentWrite) return true;
                        break;
                    case Permissions.WriteComment:
                        if (item.CommentWrite) return true;
                        break;
                }
            }

            return false;
        }
    }
}
