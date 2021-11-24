using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using TaskPlusPlus.API.Models.Task;

namespace TaskPlusPlus.API.Services
{
    public interface ITaskPlusPlusRepository
    {
        Task AddFakeData();

        #region Public
        Task<string> GetRecentChangesAsync(string accessToken);
        #endregion

        #region Users
        Task<JObject> SigninAsync(string phoneNumber, string osVersion, string deviceType, string browerVersion, string orientation);
        #endregion

        #region Boards
        Task<string> GetBoardsAsync(string accessToken);
        Task<JObject> AddBoardAsync(string accessToken, string caption);
        Task<JObject> UpdateBoardAsync(string accessToken, Guid boardId, string caption);
        Task<JObject> DeleteBoardAsync(string accessToken, Guid boardId);
        Task<JObject> ShareBoardAsync(string accessToken, Guid boardId, Guid[] shareToList);
        Task<JObject> RemoveBoardShareAsync(string accessToken, Guid boardId, Guid shareId);
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
        Task<JObject> AddCommentAsync(string accessToken, string content, Guid parentId, Guid replyTo);
        Task<string> GetCommentsAsync(string accessToken, Guid parentId);
        Task<JObject> EditCommentAsync(string accessToken, Guid parentId, Guid commentId, string text);
        Task<JObject> DeleteCommentAsync(string accessToken, Guid parentId, Guid commentId);
        #endregion

        #region Friends
        Task<JObject> AddFriendAsync(string accessToken, string phoneNumber);
        Task<string> GetFriendsListAsync(string accessToken);
        Task<string> GetFriendRequestQueueAsync(string accessToken);
        Task<JObject> ApplyFriendRequestResponceAsync(string accessToken, Guid requestId, bool reply);
        Task<JObject> RemoveFriendAsync(string accessToken, Guid requestId);
        #endregion

        #region Tags
        Task<JObject> AddTagAsync(string accessToken, Guid boardId, string caption);
        Task<string> GetTagListAsync(string accessToken, Guid parentId);
        Task<JObject> EditTagAsync(string accessToken, Guid boardId, Guid tagId, string color);
        Task<JObject> RemoveTagAsync(string accessToken, Guid boardId, Guid tagId);
        Task<JObject> AsignTagToTaskAsync(string accessToken, Guid taskId, Guid tagId);
        Task<JObject> RemoveTagFromTaskAsync(string accessToken, Guid taskId, Guid taskTagId);
        #endregion

        #region Roles
        Task<JObject> AddRoleAsync(string accessToken, Guid boardId, string caption, string color, bool readTask, bool writeTask, bool completeTask, bool readComment, bool writeComment);
        Task<JObject> EditRoleAsync(string accessToken, Guid roleId, Guid boardId, string color, bool readTask, bool writeTask, bool completeTask, bool readComment, bool writeComment);
        Task<JObject> AsignTagToRoleAsync(string accessToken, Guid boardId, Guid roleId, Guid tagId);
        Task<JObject> RemoveTagFromRoleAsync(string accessToken, Guid boardId, Guid roleTagId);
        Task<string> GetBoardRolesAsync(string accessToken, Guid boardId);
        Task<JObject> AsignRoleToEmployeesAsync(string accessToken, Guid boardId, Guid RoleId, Guid EmployeesId);
        Task<JObject> RemoveRoleFromBoardAsync(string accessToken, Guid boardId, Guid roleId);
        Task<JObject> DemoteEmployeesRoleAsync(string accessToken, Guid boardId, Guid roleSessionId);
        Task<string> GetEmployeesAsync(string accessToken, Guid boardId);
        #endregion

        #region Profile

        Task<JObject> ChangeProfileAsync(string accessToken, string firstName, string lastName, string bio, string img, string email, string phoneNumber);

        Task<string> GetProfileInfoAsync(string accessToken);

        #endregion

        #region Messages

        Task<string> GetSystemMessagesAsync(string accessToken);

        #endregion
    }
}
