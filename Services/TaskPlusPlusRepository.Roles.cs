using TaskPlusPlus.API.Entities;
using TaskPlusPlus.API.Models.Employee;
using TaskPlusPlus.API.Models.Roles;
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
        public async Task<JObject> AddRoleAsync(string accessToken, Guid boardId, string caption, string color, bool readTask, bool writeTask, bool completeTask, bool readComment, bool writeComment)
        {
            var user = await GetUserSessionAsync(accessToken);
            if (!(await IsOwnerOfBoard(user.UserId, boardId))) return JsonMap.FalseResult;

            var role = new Roles()
            {
                Id = Guid.NewGuid(),
                BoardId = boardId,
                Caption = caption,
                TaskRead = readTask || writeTask,
                TaskWrite = writeTask,
                TaskCompelete = completeTask,
                CommentRead = readComment || writeComment,
                CommentWrite = writeComment,
                Deleted = false,
                CreatedAt = DateTime.Now,
                BackgroundColor = color,
            };

            await context.Roles.AddAsync(role);
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<JObject> EditRoleAsync(string accessToken, Guid roleId, Guid boardId, string color, bool readTask, bool writeTask, bool completeTask, bool readComment, bool writeComment)
        {
            var user = await GetUserSessionAsync(accessToken);
            if (!(await IsOwnerOfBoard(user.UserId, boardId))) return JsonMap.FalseResult;
            if (!(await context.Roles.AnyAsync(r => r.Id == roleId && !r.Deleted))) return JsonMap.FalseResult;

            var role = await context.Roles.SingleAsync(r => r.Id == roleId && !r.Deleted);

            role.BackgroundColor = color;
            role.TaskRead = readTask;
            role.TaskWrite = writeTask;
            role.TaskCompelete = completeTask;
            role.CommentRead = readComment;
            role.CommentWrite = writeComment;

            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<JObject> AsignTagToRoleAsync(string accessToken, Guid boardId, Guid roleId, Guid tagId)
        {
            var user = await GetUserSessionAsync(accessToken);

            if (!(await IsOwnerOfBoard(user.UserId, boardId))) return JsonMap.FalseResult;
            if (!(await context.Roles.AnyAsync(r => r.Id == roleId && !r.Deleted))) return JsonMap.FalseResult;
            if (!(await context.Tags.AnyAsync(t => t.Id == tagId && !t.Deleted))) return JsonMap.FalseResult;

            var roleTag = new RolesTagList()
            {
                Id = Guid.NewGuid(),
                TagId = tagId,
                RoleId = roleId,
                Removed = false,
                AsignDate = DateTime.Now
            };

            await context.RolesTagList.AddAsync(roleTag);
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        private List<RoleTag> GetRoleTags(Guid boardId, Guid roleId)
        {
            var jsonData = new List<RoleTag>();

            var res = from roleTags in context.RolesTagList.Where(r => r.RoleId == roleId && !r.Removed).OrderBy(r => r.AsignDate)
                      select new
                      {
                          roleTags.Id,
                          roleTags.TagId
                      };

            foreach(var item in res)
            {
                var tag = context.Tags.Single(t => t.Id == item.TagId);

                jsonData.Add(new RoleTag()
                {
                    RoleTagId = item.Id,
                    TagId =  tag.Id,
                    Caption = tag.Caption,
                    Color = tag.BackgroundColor,
                });
            }

            return jsonData;
        }

        public async Task<JObject> RemoveTagFromRoleAsync(string accessToken, Guid boardId, Guid roleTagId)
        {
            var user = await GetUserSessionAsync(accessToken);

            if (!(await IsOwnerOfBoard(user.UserId, boardId))) return JsonMap.FalseResult;
            if (!(await context.RolesTagList.AnyAsync(r => r.Id == roleTagId && !r.Removed))) return JsonMap.FalseResult;

            var roleTag = await context.RolesTagList.SingleAsync(r => r.Id == roleTagId && !r.Removed);

            roleTag.Removed = true;

            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<string> GetBoardRolesAsync(string accessToken, Guid boardId)
        {
            var user = await GetUserSessionAsync(accessToken);

            var jsonData = new JArray();
            if (!(await context.SharedBoards.AnyAsync(s => s.ShareTo == user.UserId && s.BoardId == boardId && !s.Deleted))) return jsonData.ToString();

            var res = from roles in context.Roles.Where(r => r.BoardId == boardId && !r.Deleted).OrderBy(s => s.CreatedAt)
                      select new
                      {
                          roles.Id,
                          roles.Caption,
                          roles.TaskRead,
                          roles.TaskWrite,
                          roles.CommentRead,
                          roles.CommentWrite,
                          roles.TaskCompelete,
                          roles.BackgroundColor,
                      };

            foreach (var item in res)
            {
                jsonData.Add(new JObject
                {
                    {"Id",item.Id},
                    {"Caption",item.Caption},
                    {"ReadTask",item.TaskRead},
                    {"WriteTask",item.TaskWrite},
                    {"ReadComment",item.CommentRead},
                    {"WriteComment",item.CommentWrite},
                    {"CompleteTask", item.TaskCompelete },
                    {"Color", item.BackgroundColor},
                    {"Tags", JToken.FromObject(GetRoleTags(boardId,item.Id))}
                });
            }

            return jsonData.ToString();
        }

        public async Task<JObject> AsignRoleToEmployeesAsync(string accessToken, Guid boardId, Guid roleId, Guid employeesId)
        {
            var user = await GetUserSessionAsync(accessToken);
            var isOwner = await IsOwnerOfBoard(user.UserId, boardId);
            var selfPromote = await IsOwnerOfBoard(employeesId, boardId);
            var isShared = await context.SharedBoards.AnyAsync(s => s.BoardId == boardId && s.ShareTo == employeesId && !s.Deleted);
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

        private async Task<List<EmployeeRoles>> GetEmployeesRolesAsync(Guid userId, Guid boardId)
        {  
            var jsonData = new List<EmployeeRoles>();

            var roleSessions = from roleSession in context.RoleSessions.Where(r => userId == r.UserId && r.BoardId == boardId && !r.Demoted).OrderBy(r => r.AsignDate)
                               select new
                               {
                                   roleSession.Id,
                                   roleSession.RoleId,
                               };

            foreach (var item in roleSessions)
            {
                var role = await context.Roles.SingleOrDefaultAsync(r => r.Id == item.RoleId);

                jsonData.Add(new EmployeeRoles()
                {
                    RoleId = role.Id,
                    RoleSessionId = item.Id,
                    Caption = role.Caption,
                    Color = role.BackgroundColor,
                });
            }

            return jsonData;
        }

        public async Task<string> GetEmployeesAsync(string accessToken, Guid boardId)
        {
            var user = await GetUserSessionAsync(accessToken);
            var board = await context.Boards.SingleOrDefaultAsync(b => b.Id == boardId);
            var isShared = await context.SharedBoards.AnyAsync(s => s.BoardId == boardId && s.ShareTo == user.UserId && !s.Deleted);

            var jsonData = new JArray();
            if (board == null || !isShared) return jsonData.ToString();

            var res = from sharedBoard in context.SharedBoards.Where(s => s.BoardId == boardId && s.ShareTo != board.CreatorId && !s.Deleted).OrderBy(s => s.GrantedAccessAt)
                      select new
                      {
                          sharedBoard.ShareTo,
                          sharedBoard.Id,
                      };

            foreach (var item in res)
            {
                var shareTo = await GetUser(item.ShareTo);

                jsonData.Add(new JObject {
                    {"ShareId", item.Id},
                    {"Id", shareTo.UserId},
                    {"FirstName", shareTo.FirstName},
                    {"LastName", shareTo.LastName},
                    {"Bio", shareTo.Bio},
                    {"Image", shareTo.Image},
                    {"Roles", JToken.FromObject(await GetEmployeesRolesAsync(shareTo.UserId,boardId))}
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
