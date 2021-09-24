﻿using TaskPlusPlus.API.Entities;
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
            if (!await IsOwnerOfBoard(user.UserId, taskId)) return JsonMap.FalseResult;
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
            if (!(await IsOwnerOfBoard(user.UserId, taskId))) return JsonMap.FalseResult;

            var taskTag = await context.TagsList.SingleOrDefaultAsync(t => t.Id == taskTagId && !t.Removed);
            if (taskTag == null) return JsonMap.FalseResult;

            taskTag.Removed = true;
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