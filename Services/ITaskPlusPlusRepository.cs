using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using TaskPlusPlus.API.Entities;

namespace TaskPlusPlus.API.Services
{
    public interface ITaskPlusPlusRepository
    {
        Task<JObject> SignUpAsync(string firstName, string lastName, string phoneNumber);
        Task<JObject> SigninAsync(string phoneNumber);
        
        Task<string> GetBoardsAsync(string accessToken);
        Task<JObject> AddBoardAsync(string accessToken, string caption);
        Task<JObject> UpdateBoardAsync(string accessToken, Guid boardId, string caption);
        Task<JObject> DeleteBoardAsync(string accessToken, Guid boardId);

        Task<string> GetTasksAsync(string accessToken, Guid parentId);
        Task<JObject> AddTaskAsync(string accessToken, Guid parentId, string caption);
        Task<JObject> EditTaskAsync(string accessToken, Guid parentId, string caption, bool star);
        Task<JObject> AddSubTaskAsync(string accessToken, Guid parentId, string caption);
        Task<JObject> EditSubTaskAsync(string accessToken, Guid parentId, string caption, bool star);

        Task<JObject> DeleteTaskAsync(string accessToken, Guid parentId);

        Task<JObject> AddCommentAsync(string accessToken, Guid parentId, string text);


        Task<string> GetCommentsAsync(string accessToken, Guid parentId);

        Task<JObject> EditCommentAsync(string accessToken, Guid parentId, Guid commentId, string text);

        Task<JObject> DeleteCommentAsync(string accessToken, Guid parentId, Guid commentId);

        Task<JObject> AddFriendAsync(string accessToken, string phoneNumber);

        Task<string> GetFriendsListAsync(string accessToken);

        Task<string> GetFriendRequestQueueAsync(string accessToken);

        Task<JObject> ApplyFriendRequestAsync(string accessToken, Guid requestId, bool reply);


        Task<JObject> RemoveFriendAsync(string accessToken, Guid requestId);


        Task<JObject> ShareBoardAsync(string accessToken,Guid boardId,string shareToList);
    }
}
