using TaskPlusPlus.API.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using TaskPlusPlus.API.Models.Comment;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaskPlusPlus.API.DbContexts;

namespace TaskPlusPlus.API.Services
{
    public partial class TaskPlusPlusRepository
    {
        public async Task<JObject> AddCommentAsync(string accessToken, string content, Guid parentId, Guid replyTo)
        {
            using var context = new TaskPlusPlusContext();
            var user = await GetUserSessionAsync(accessToken, context);
            var isOwner = await IsOwnerOfBoardAsync(user.UserId, parentId, context);
            if (await HaveAccessToTaskَAsync(user.UserId, parentId, context) == false) return JsonMap.FalseResult;
            if (!isOwner && !(await HasPermissionsAsync(user.UserId, parentId, Permissions.WriteComment, context))) return JsonMap.FalseResult;

            var comment = new Comment()
            {
                Id = Guid.NewGuid(),
                Text = content,
                Sender = user.UserId,
                ReplyTo = replyTo,
                ParentId = parentId,
                CreationDate = DateTime.Now,
                Deleted = false,
                EditId = Guid.NewGuid(),
                LastModifiedBy = user.UserId
            };


            if (replyTo.ToString() == "00000000-0000-0000-0000-000000000000")
                comment.ReplyTo = comment.Id;

            comment.EditId = comment.Id;


            await context.Comments.AddAsync(comment);
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<string> GetCommentsAsync(string accessToken, Guid parentId)
        {
            using var context = new TaskPlusPlusContext();
            var user = await GetUserSessionAsync(accessToken, context);
            var isOwner = await IsOwnerOfBoardAsync(user.UserId, parentId, context);
            if (await HaveAccessToTaskَAsync(user.UserId, parentId, context) == false) return JsonMap.FalseResult.ToString();
            if (!isOwner && !await HasPermissionsAsync(user.UserId, parentId, Permissions.ReadComment, context)) return JsonMap.FalseResult.ToString();

            var res = from comment in context.Comments
                      .Where(c => c.ParentId == parentId && !c.Deleted && c.EditId == c.Id).OrderBy(c => c.CreationDate)
                      select new
                      {
                          comment.Id,
                          comment.Text,
                          comment.Sender,
                          comment.CreationDate,
                          comment.LastModifiedBy,
                          comment.ReplyTo
                      };

            var jsonData = new JArray();
            foreach (var item in res)
            {
                var replyInfo = await GetReplyInfo(item.ReplyTo, item.Id, context);
                jsonData.Add(new JObject
                    {
                        { "Id", item.Id },
                        { "Content",  item.Text },
                        { "CreationAt",  item.CreationDate },
                        { "Sender",(await GetUser(item.Sender, context)).FirstName },
                        { "Reply", replyInfo.Content},
                        { "ReplySender", replyInfo.Sender},
                        { "LastModifiedBy", (await GetUser(item.LastModifiedBy, context)).FirstName }
                    });
            }
            return jsonData.ToString();
        }

        public async Task<JObject> EditCommentAsync(string accessToken, Guid parentId, Guid commentId, string text)
        {
            using var context = new TaskPlusPlusContext();
            var user = await GetUserSessionAsync(accessToken, context);
            var isOwner = await IsOwnerOfBoardAsync(user.UserId, parentId, context);
            if (await HaveAccessToTaskَAsync(user.UserId, parentId, context) == false) return JsonMap.FalseResult;

            //find comment => create new comment => change edit id value to new comment id => save data base

            var oldComment = await context.Comments.SingleOrDefaultAsync(c => c.Id == commentId && (c.Sender == user.UserId || isOwner));
            if (oldComment == null) return JsonMap.FalseResult;

            var comment = new Comment()
            {
                Id = Guid.NewGuid(),
                Text = text,
                Sender = oldComment.Sender,
                ReplyTo = oldComment.ReplyTo,
                CreationDate = oldComment.CreationDate,
                Deleted = false,
                EditId = Guid.NewGuid(),
                LastModifiedBy = user.UserId,
                ParentId = oldComment.ParentId,
            };

            if (oldComment.Id == oldComment.ReplyTo)
                comment.ReplyTo = comment.Id;

            oldComment.EditId = comment.Id;
            comment.EditId = comment.Id;


            await context.Comments.AddAsync(comment);
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }

        public async Task<JObject> DeleteCommentAsync(string accessToken, Guid parentId, Guid commentId)
        {
            using var context = new TaskPlusPlusContext();
            var user = await GetUserSessionAsync(accessToken, context);
            var isOwner = await IsOwnerOfBoardAsync(user.UserId, parentId, context);
            if (await HaveAccessToTaskَAsync(user.UserId, parentId, context) == false) return JsonMap.FalseResult;

            var comment = await context.Comments.SingleOrDefaultAsync(c => c.Id == commentId && (c.Sender == user.UserId || isOwner));

            if (comment == null) return JsonMap.FalseResult;

            comment.Deleted = true;
            comment.LastModifiedBy = user.UserId;
            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }


        private async Task<ReplyInfo> GetReplyInfo(Guid replyId, Guid id, TaskPlusPlusContext context)
        {
            ReplyInfo replyInfo = new ReplyInfo()
            {
                Sender = string.Empty,
                Content = string.Empty
            };

            if (id == replyId)
                return replyInfo;

            if (!(await context.Comments.AnyAsync(c => !c.Deleted && c.Id == replyId)))
                return replyInfo;

            var reply = await context.Comments.SingleAsync(c => !c.Deleted && c.Id == replyId);

            if (reply.EditId != reply.Id)
            {
                if (!(await context.Comments.AnyAsync(c => !c.Deleted && c.Id == reply.EditId)))
                    return replyInfo;

                reply = await context.Comments.SingleAsync(c => !c.Deleted && c.Id == reply.EditId);
            }

            replyInfo.Sender = (await GetUser(reply.Sender, context)).FirstName;
            replyInfo.Content = reply.Text;


            return replyInfo;
        }
    }
}
