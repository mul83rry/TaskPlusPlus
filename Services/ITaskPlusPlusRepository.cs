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

        Task<JObject> AddTagAsync(string accessToken, Guid boardId, string caption);

        Task<string> GetTagListAsync(string accessToken, Guid parentId);

        Task<JObject> RemoveTagAsync(string accessToken, Guid boardId, Guid tagId);

        Task<JObject> AsignTagToTaskAsync(string accessToken, Guid taskId, Guid tagId);

        Task<JObject> RemoveTagFromTaskAsync(string accessToken, Guid taskId, Guid taskTagId);

        Task<JObject> AddRoleAsync(string accessToken, Guid boardId, string caption, bool readTask, bool writeTask, bool readComment, bool writeComment, string tagList);

        Task<string> GetBoardRolesAsync(string accessToken, Guid boardId);

        Task<JObject> AsignRoleToEmployeesAsync(string accessToken, Guid boardId, Guid RoleId, Guid EmployeesId);

        Task<JObject> RemoveRoleFromBoardAsync(string accessToken, Guid boardId, Guid roleId);

        Task<JObject> DemoteEmployeesAsync(string accessToken, Guid boardId, Guid roleSessionId);

        Task<string> GetEmployeesRolesAsync(string accessToken, Guid boardId);

        Task<string> GetEmployeesAsync(string accessToken, Guid boardId);
    }
}
