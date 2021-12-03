using TaskPlusPlus.API.Entities;
using TaskPlusPlus.API.Models.Task;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using TaskPlusPlus.API.DbContexts;

namespace TaskPlusPlus.API.Services
{
    public partial class TaskPlusPlusRepository
    {
        public async Task<JObject> AddTagAsync(string accessToken, Guid boardId, string caption)
        {
            using var context = new TaskPlusPlusContext();
            var user = await GetUserSessionAsync(accessToken, context);
            var board = await context.Boards.SingleOrDefaultAsync(b => b.Id == boardId && b.CreatorId == user.UserId);
            if (board == null) return JsonMap.FalseResult;

            var tag = new Tag()
            {
                Id = Guid.NewGuid(),
                BoardId = boardId,
                Caption = caption,
                Deleted = false,
                CreationDate = DateTime.Now,
                BackgroundColor = "#a244fa", //todo: check
            };

            await context.Tags.AddAsync(tag);
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<string> GetTagListAsync(string accessToken, Guid parentId)
        {
            using var context = new TaskPlusPlusContext();
            var user = await GetUserSessionAsync(accessToken, context);
            var boardId = await GetBoardIdAsync(parentId, context);

            var res = from tag in context.Tags.Where(t => !t.Deleted).OrderBy(t => t.CreationDate)
                      join sharedBoard in context.SharedBoards
                      .Where(shared => shared.ShareTo == user.UserId && boardId == shared.BoardId && !shared.Deleted)
                      on tag.BoardId equals sharedBoard.BoardId
                      select new
                      {
                          tag.Id,
                          tag.Caption,
                          tag.BackgroundColor
                      };

            var jsonData = new JArray();
            foreach (var item in res)
            {

                jsonData.Add(new JObject
                {
                   {"Id", item.Id },
                   {"Caption",  item.Caption },
                   {"Color", item.BackgroundColor}
                });
            }
            return jsonData.ToString();
        }

        public async Task<JObject> AsignTagToTaskAsync(string accessToken, Guid taskId, Guid tagId)
        {
            using var context = new TaskPlusPlusContext();
            var user = await GetUserSessionAsync(accessToken, context);
            if (!await IsOwnerOfBoardAsync(user.UserId, taskId, context)) return JsonMap.FalseResult;
            if (!await context.Tags.AnyAsync(t => t.Id == tagId && !t.Deleted)) return JsonMap.FalseResult;
            if (!await context.Tasks.AnyAsync(t => t.Id == taskId && !t.Deleted)) return JsonMap.FalseResult;
            if (await context.TagsList.AnyAsync(t => !t.Deleted && t.TagId == tagId && t.TaskId == taskId)) return JsonMap.FalseResult;

            var taskTag = new TagsList()
            {
                Id = Guid.NewGuid(),
                TagId = tagId,
                TaskId = taskId,
                Deleted = false,
                AsignDate = DateTime.Now
            };

            await context.TagsList.AddAsync(taskTag);
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        private async Task<List<TaskTags>> GetTaskTagListAsync(Guid taskId, TaskPlusPlusContext context)
        {
            var res = from tagList in context.TagsList.Where(t => t.TaskId == taskId && !t.Deleted).OrderBy(t => t.AsignDate)
                      select new
                      {
                          tagList.Id,
                          tagList.TagId,
                      };

            var data = new List<TaskTags>();
            foreach (var item in res)
            {
                if (!(await context.Tags.AnyAsync(t => t.Id == item.TagId && !t.Deleted))) continue;

                var tag = await context.Tags.SingleAsync(t => t.Id == item.TagId && !t.Deleted);
                var listItem = new TaskTags()
                {
                    TagListId = item.Id,
                    TagId = item.TagId,
                    Caption = tag.Caption,
                    Color = tag.BackgroundColor
                };
                data.Add(listItem);
            }

            return data;
        }

        public async Task<JObject> RemoveTagAsync(string accessToken, Guid boardId, Guid tagId)
        {
            using var context = new TaskPlusPlusContext();
            var user = await GetUserSessionAsync(accessToken, context);
            var isOwner = await IsOwnerOfBoardAsync(user.UserId, boardId, context);
            var isUsing = await TagIsUsing(tagId);

            if (!isOwner || isUsing) return JsonMap.FalseResult; //todo: check

            var tag = await context.Tags.SingleOrDefaultAsync(t => t.Id == tagId);
            tag.Deleted = true;
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<JObject> EditTagAsync(string accessToken, Guid boardId, Guid tagId, string color)
        {
            using var context = new TaskPlusPlusContext();
            var user = await GetUserSessionAsync(accessToken, context);
            var isOwner = await IsOwnerOfBoardAsync(user.UserId, boardId, context);

            if (!isOwner) return JsonMap.FalseResult;

            var tag = await context.Tags.SingleOrDefaultAsync(t => t.Id == tagId);

            tag.BackgroundColor = color;
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<JObject> RemoveTagFromTaskAsync(string accessToken, Guid taskId, Guid taskTagId)
        {
            using var context = new TaskPlusPlusContext();
            var user = await GetUserSessionAsync(accessToken, context);
            if (!(await IsOwnerOfBoardAsync(user.UserId, taskId, context))) return JsonMap.FalseResult;

            var taskTag = await context.TagsList.SingleOrDefaultAsync(t => t.Id == taskTagId && !t.Deleted);
            if (taskTag == null) return JsonMap.FalseResult;

            taskTag.Deleted = true;
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<bool> TagIsUsing(Guid tagId)
        {
            using var context = new TaskPlusPlusContext();
            
            var tagIsUsingInTask = await (from tasksTag in context.TagsList.Where(t => t.TagId == tagId)
                              join task in context.Tasks.Where(t => !t.Deleted) on tasksTag.TaskId equals task.Id
                              select new
                              {
                                  tasksTag.Id,
                              }).AnyAsync();

            var tagIsUsingInRole = await (from rolesTag in context.RolesTagList.Where(r => r.TagId == tagId)
                               join role in context.Roles.Where(r => !r.Deleted) on rolesTag.RoleId equals role.Id
                               select new
                               {
                                   rolesTag.RoleId,
                               }).AnyAsync();

            return tagIsUsingInTask || tagIsUsingInRole;
        }

        /*public async Task<bool> TagIsUsing(Guid tagId) :/
        {
            using var context = new TaskPlusPlusContext();
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
                              join role in context.Roles.Where(r => !r.Deleted) on rolesTag.RoleId equals role.Id
                              select new
                              {
                                  rolesTag.RoleId,
                              };

            if (await tagsInRoles.AnyAsync()) tagIsUsingInRole = true;
            if (await tagsInTasks.AnyAsync()) tagIsUsingInTask = true;

            return tagIsUsingInTask || tagIsUsingInRole;
        }*/

        private static async Task<bool> HasTagAccess(Guid boardId, Guid userId, Guid parentId, TaskPlusPlusContext context)
        {
            var hasRole = await context.RoleSessions.AnyAsync(r => !r.Demoted && r.BoardId == boardId && r.UserId == userId);
            if (!hasRole) return true;

            var rolesTags = from roleSession in context.RoleSessions.Where(r => !r.Demoted && r.BoardId == boardId && r.UserId == userId)
                            join roleTags in context.RolesTagList.Where(r => !r.Removed) on roleSession.RoleId equals roleTags.RoleId
                            select new
                            {
                                roleTags.Id,
                                roleTags.TagId,
                                roleTags.RoleId
                            };

            var taskTags = from tasktag in context.TagsList.Where(t => !t.Deleted && t.TaskId == parentId)
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
