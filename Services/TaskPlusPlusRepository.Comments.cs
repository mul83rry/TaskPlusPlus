using TaskPlusPlus.API.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace TaskPlusPlus.API.Services
{
    public partial class TaskPlusPlusRepository
    {
        public async Task<JObject> AddCommentAsync(string accessToken, Guid parentId, string text)
        {
            var user = await GetUserSessionAsync(accessToken);
            var isOwner = await IsOwnerOfBoard(user.UserId, parentId);
            if (await HaveAccessToTask(user.UserId, parentId) == false) return JsonMap.FalseResult;
            if (!isOwner && !(await HasPermissions(user.UserId, parentId, Permissions.WriteComment))) return JsonMap.FalseResult;

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
            var isOwner = await IsOwnerOfBoard(user.UserId, parentId);
            if (await HaveAccessToTask(user.UserId, parentId) == false) return JsonMap.FalseResult.ToString();
            if (!isOwner && !await HasPermissions(user.UserId, parentId, Permissions.ReadComment)) return JsonMap.FalseResult.ToString();

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

        public async Task<JObject> EditCommentAsync(string accessToken, Guid parentId, Guid commentId, string text)
        {
            var user = await GetUserSessionAsync(accessToken);
            var isOwner = await IsOwnerOfBoard(user.UserId, parentId);
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
            var isOwner = await IsOwnerOfBoard(user.UserId, parentId);
            if (await HaveAccessToTask(user.UserId, parentId) == false) return JsonMap.FalseResult;

            var comment = await context.Comments.SingleAsync(c => c.Id == commentId && (c.Sender == user.UserId || isOwner));

            comment.Deleted = true;
            comment.LastModifiedBy = user.UserId;
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }
    }
}
