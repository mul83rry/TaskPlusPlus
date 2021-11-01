using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace TaskPlusPlus.API.Services
{
    public interface ITaskPlusPlusRepository
    {
        #region Users
        Task<JObject> SigninAsync(string phoneNumber, string osVersion, string deviceType, string browerVersion, string orientation);
        #endregion

        #region Boards
        Task<string> GetBoardsAsync(string accessToken);
        Task<JObject> AddBoardAsync(string accessToken, string caption);
        Task<JObject> UpdateBoardAsync(string accessToken, Guid boardId, string caption);
        Task<JObject> DeleteBoardAsync(string accessToken, Guid boardId);
        Task<JObject> ShareBoardAsync(string accessToken, Guid boardId, string shareToList);
        #endregion

        #region Tasks
        Task<string> GetTasksAsync(string accessToken, Guid parentId);
        Task<JObject> AddTaskAsync(string accessToken, Guid parentId, string caption);
        Task<JObject> EditTaskAsync(string accessToken, Guid parentId, string caption, bool star);
        Task<JObject> AddSubTaskAsync(string accessToken, Guid parentId, string caption);
        Task<JObject> EditSubTaskAsync(string accessToken, Guid parentId, string caption, bool star);

        Task<JObject> CompeleteTaskAsync(string accessToken, Guid parentId);

        Task<JObject> DeleteTaskAsync(string accessToken, Guid parentId);
        #endregion

        #region comments
        Task<JObject> AddCommentAsync(string accessToken, Guid parentId, string text);
        Task<string> GetCommentsAsync(string accessToken, Guid parentId);
        Task<JObject> EditCommentAsync(string accessToken, Guid parentId, Guid commentId, string text);
        Task<JObject> DeleteCommentAsync(string accessToken, Guid parentId, Guid commentId);
        #endregion

        #region Friends
        Task<JObject> AddFriendAsync(string accessToken, string phoneNumber);
        Task<string> GetFriendsListAsync(string accessToken);
        Task<string> GetFriendRequestQueueAsync(string accessToken);
        Task<JObject> ApplyFriendRequestAsync(string accessToken, Guid requestId, bool reply);
        Task<JObject> RemoveFriendAsync(string accessToken, Guid requestId);
        #endregion

        #region Tags
        Task<JObject> AddTagAsync(string accessToken, Guid boardId, string caption);
        Task<string> GetTagListAsync(string accessToken, Guid parentId);
        Task<JObject> RemoveTagAsync(string accessToken, Guid boardId, Guid tagId);
        Task<JObject> AsignTagToTaskAsync(string accessToken, Guid taskId, Guid tagId);
        Task<JObject> RemoveTagFromTaskAsync(string accessToken, Guid taskId, Guid taskTagId);
        #endregion

        #region Roles
        Task<JObject> AddRoleAsync(string accessToken, Guid boardId, string caption, bool readTask, bool writeTask, bool readComment, bool writeComment, string tagList);
        Task<string> GetBoardRolesAsync(string accessToken, Guid boardId);
        Task<JObject> AsignRoleToEmployeesAsync(string accessToken, Guid boardId, Guid RoleId, Guid EmployeesId);
        Task<JObject> RemoveRoleFromBoardAsync(string accessToken, Guid boardId, Guid roleId);
        Task<JObject> DemoteEmployeesRoleAsync(string accessToken, Guid boardId, Guid roleSessionId);
        Task<string> GetEmployeesRolesAsync(string accessToken, Guid boardId);
        Task<string> GetEmployeesAsync(string accessToken, Guid boardId);
        #endregion
    }
}
