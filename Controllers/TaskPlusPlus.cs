using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TaskPlusPlus.API.Models;
using TaskPlusPlus.API.Models.Task;
using TaskPlusPlus.API.Models.Board;
using TaskPlusPlus.API.Models.Comment;
using TaskPlusPlus.API.Models.Friend;
using TaskPlusPlus.API.Models.Tag;
using TaskPlusPlus.API.Models.Roles;
using TaskPlusPlus.API.Models.Employee;
using TaskPlusPlus.API.Models.Profile;
using TaskPlusPlus.API.Models.Messages;
using TaskPlusPlus.API.Services;
using System.Web.Http.Cors;

namespace TaskPlusPlus.API.Controllers
{
    [Route("api")]
    [ApiController]
    [EnableCors(origins: "*", headers: "*", methods: "*", exposedHeaders: "Access-Control-Allow-Origin")]
    public class TaskPlusPlus : ControllerBase
    {
        private readonly ITaskPlusPlusRepository repository;

        public TaskPlusPlus(ITaskPlusPlusRepository repo) => repository = repo;

        [HttpGet]
        public IActionResult Welcome() => Ok("welcome to task++");

        #region Login

        [HttpPost]
        [Route(EventsKey.Signin)]
        public async Task<IActionResult> SigninAsync([FromBody] SignIn signIn) =>
            Ok(await repository.SigninAsync(signIn.PhoneNumber, signIn.OsVersion, signIn.DeviceType, signIn.BrowerVersion, signIn.Orientation));
        
        #endregion

        #region Board

        [HttpPost]
        [Route(EventsKey.AddBoard)]
        public async Task<IActionResult> AddBoardAsync([FromBody] AddBoard board) =>
            Ok(await repository.AddBoardAsync(board.AccessToken, board.Caption));

        [HttpPost]
        [Route(EventsKey.EditBoard)]
        public async Task<IActionResult> UpdateBoardAsync([FromBody] EditBoard newBoard) =>
            Ok(await repository.UpdateBoardAsync(newBoard.AccessToken, newBoard.Id, newBoard.Caption));

        [HttpPost]
        [Route(EventsKey.DeleteBoard)]
        public async Task<IActionResult> DeleteBoardAsync([FromBody] DeleteBoard board) =>
            Ok(await repository.DeleteBoardAsync(board.AccessToken, board.Id));

        [HttpPost]
        [Route(EventsKey.GetBoards)]
        public async Task<IActionResult> GetBoardsListAsync([FromBody] GetBoard accessToken) =>
            Ok(await repository.GetBoardsAsync(accessToken.AccessToken));

        [HttpPost]
        [Route(EventsKey.ShareBoard)]
        public async Task<IActionResult> ShareBoardAsync([FromBody] ShareBoard share) =>
            Ok(await repository.ShareBoardAsync(share.AccessToken, share.BoardId, share.ShareToList));

        [HttpPost]
        [Route(EventsKey.RemoveShare)]
        public async Task<IActionResult> RemoveBoardShareAsync([FromBody] RemoveEmployee employee) =>
                    Ok(await repository.RemoveBoardShareAsync(employee.AccessToken, employee.BoardId, employee.ShareId));

        #endregion
        
        #region Task

        [HttpPost]
        [Route(EventsKey.GetTasks)]
        public async Task<IActionResult> GetTaskAsync([FromBody] GetTask task) =>
            Ok(await repository.GetTasksAsync(task.AccessToken, task.ParentId));

        [HttpPost]
        [Route(EventsKey.AddTask)]
        public async Task<IActionResult> AddTaskAsync([FromBody] AddTask task) =>
            Ok(await repository.AddTaskAsync(task.AccessToken, task.ParentId, task.Caption));

        [HttpPost]
        [Route(EventsKey.EditTask)]
        public async Task<IActionResult> EditTaskAsync([FromBody] EditTask task) =>
            Ok(await repository.EditTaskAsync(task.AccessToken, task.Id, task.Caption, task.Star));

        [HttpPost]
        [Route(EventsKey.AddsubTask)]
        public async Task<IActionResult> AddSubTaskAsync([FromBody] AddTask task) =>
            Ok(await repository.AddSubTaskAsync(task.AccessToken, task.ParentId, task.Caption));

        [HttpPost]
        [Route(EventsKey.EditSubtask)]
        public async Task<IActionResult> EditSubTaskAsync([FromBody] EditTask task) =>
            Ok(await repository.EditSubTaskAsync(task.AccessToken, task.Id, task.Caption, task.Star));

        [HttpPost]
        [Route(EventsKey.CompeleteTask)]
        public async Task<IActionResult> CompeletTaskAsync([FromBody] CompeleteTask task) =>
            Ok(await repository.CompeleteTaskAsync(task.AccessToken, task.Id));

        [HttpPost]
        [Route(EventsKey.DeleteTask)]
        public async Task<IActionResult> DeleteTaskAsync([FromBody] DeleteTask task) =>
            Ok(await repository.DeleteTaskAsync(task.AccessToken, task.Id));

        #endregion

        #region Comment

        [HttpPost]
        [Route(EventsKey.AddComment)]
        public async Task<IActionResult> AddCommentAsync([FromBody] AddComment comment) =>
            Ok(await repository.AddCommentAsync(comment.AccessToken, comment.Content, comment.ParentId, comment.ReplyTo));


        [HttpPost]
        [Route(EventsKey.GetComments)]
        public async Task<IActionResult> GetCommentsAsync([FromBody] GetComment comment) =>
            Ok(await repository.GetCommentsAsync(comment.AccessToken, comment.ParentId));


        [HttpPost]
        [Route(EventsKey.EditComment)]
        public async Task<IActionResult> EditCommentAsync([FromBody] EditComment comment) =>
            Ok(await repository.EditCommentAsync(comment.AccessToken, comment.ParentId, comment.Id, comment.Content));


        [HttpPost]
        [Route(EventsKey.DeleteComment)]
        public async Task<IActionResult> DeleteCommentAsync([FromBody] DeleteComment comment) =>
            Ok(await repository.DeleteCommentAsync(comment.AccessToken, comment.ParentId, comment.Id));

        #endregion

        #region Friend

        [HttpPost]
        [Route(EventsKey.AddFriend)]
        public async Task<IActionResult> AddFriendAsync([FromBody] AddFriend friend) =>
            Ok(await repository.AddFriendAsync(friend.AccessToken, friend.PhoneNumber));

        [HttpPost]
        [Route(EventsKey.GetFriends)]
        public async Task<IActionResult> GetFriendListAsync([FromBody] GetFriendList friend) =>
            Ok(await repository.GetFriendsListAsync(friend.AccessToken));

        [HttpPost]
        [Route(EventsKey.GetFriendRequests)]
        public async Task<IActionResult> GetFriendsRequestQueueAsync([FromBody] GetFriendRequest friend) =>
            Ok(await repository.GetFriendRequestQueueAsync(friend.AccessToken));

        [HttpPost]
        [Route(EventsKey.ApplyRequestResponce)]
        public async Task<IActionResult> ApplyFriendRequestResponceAsync([FromBody] RequestResponce request) =>
            Ok(await repository.ApplyFriendRequestResponceAsync(request.AccessToken, request.Id, request.Responce));

        [HttpPost]
        [Route(EventsKey.RemoveFriend)]
        public async Task<IActionResult> RemoveFriendAsync([FromBody] RemoveFriend request) =>
            Ok(await repository.RemoveFriendAsync(request.AccessToken, request.Id));

        #endregion

        #region Tag

        [HttpPost]
        [Route(EventsKey.AddTag)]
        public async Task<IActionResult> AddTagAsync([FromBody] AddTag tag) =>
            Ok(await repository.AddTagAsync(tag.AccessToken, tag.BoardId, tag.Caption));

        [HttpPost]
        [Route(EventsKey.GetBoardsTag)]
        public async Task<IActionResult> GetTagListAsync([FromBody] GetTag tag) =>
            Ok(await repository.GetTagListAsync(tag.AccessToken, tag.BoardId));

        [HttpPost]
        [Route(EventsKey.RemoveTag)]
        public async Task<IActionResult> RemoveTagAsync([FromBody] RemoveTag tag) =>
            Ok(await repository.RemoveTagAsync(tag.AccessToken, tag.BoardId, tag.Id));

        [HttpPost]
        [Route(EventsKey.AsignTag)]
        public async Task<IActionResult> AsignTagToTaskAsync([FromBody] AsignTag tag) =>
            Ok(await repository.AsignTagToTaskAsync(tag.AccessToken, tag.TaskId, tag.Id));

        [HttpPost]
        [Route(EventsKey.RemoveAssignment)]
        public async Task<IActionResult> RemoveTagFromTaskAsync([FromBody] RemoveAssignment tag) =>
            Ok(await repository.RemoveTagFromTaskAsync(tag.AccessToken, tag.TaskId, tag.TaskTagId));

        [HttpPost]
        [Route(EventsKey.EditTag)]
        public async Task<IActionResult> EditTagAsync([FromBody] EditTag tag) =>
            Ok(await repository.EditTagAsync(tag.AccessToken, tag.BoardId, tag.Id, tag.Color));

        #endregion

        #region Role

        [HttpPost]
        [Route(EventsKey.AddRole)]
        public async Task<IActionResult> AddRoleAsync([FromBody] AddRole role) =>
            Ok(await repository.AddRoleAsync(role.AccessToken, role.BoardId, role.Caption, role.Color,
                role.ReadTask, role.WriteTask, role.CompleteTask, role.ReadComment, role.WriteComment));

        [HttpPost]
        [Route(EventsKey.AsignTagtoRole)]
        public async Task<IActionResult> AsignTagToRoleAsync([FromBody] AsignTagRole role) =>
            Ok(await repository.AsignTagToRoleAsync(role.AccessToken, role.BoardId, role.RoleId, role.TagId));

        [HttpPost]
        [Route(EventsKey.RemoveTagFromRole)]
        public async Task<IActionResult> RemoveTagFromRoleAsync([FromBody] RemoveTagRole role) =>
            Ok(await repository.RemoveTagFromRoleAsync(role.AccessToken, role.BoardId, role.RoleTagId));

        [HttpPost]
        [Route(EventsKey.EditRole)]
        public async Task<IActionResult> EditRoleAsync([FromBody] EditRole role) =>
            Ok(await repository.EditRoleAsync(role.AccessToken, role.Id, role.BoardId, role.Color, role.ReadTask,
                role.WriteTask, role.CompleteTask, role.ReadComment, role.WriteComment));

        [HttpPost]
        [Route(EventsKey.GetBoardRoles)]
        public async Task<IActionResult> GetBoardRolesAsync([FromBody] GetRole role) =>
            Ok(await repository.GetBoardRolesAsync(role.AccessToken, role.BoardId));

        [HttpPost]
        [Route(EventsKey.AsignRole)]
        public async Task<IActionResult> AsignRoleToEmployeesAsync([FromBody] AsignRole role) =>
            Ok(await repository.AsignRoleToEmployeesAsync(role.AccessToken, role.BoardId, role.RoleId, role.EmployeeId));

        [HttpPost]
        [Route(EventsKey.Demote)]
        public async Task<IActionResult> DemoteEmployeesAsync([FromBody] Demote role) =>
            Ok(await repository.DemoteEmployeesRoleAsync(role.AccessToken, role.BoardId, role.RoleSessionId));

        [HttpPost]
        [Route(EventsKey.RemoveRole)]
        public async Task<IActionResult> RemoveRoleFromBoardAsync([FromBody] RemoveRole role) =>
            Ok(await repository.RemoveRoleFromBoardAsync(role.AccessToken, role.BoardId, role.Id));

        [HttpPost]
        [Route(EventsKey.GetEmployees)]
        public async Task<IActionResult> GetEmployeesAsync([FromBody] GetEmployee employee) =>
            Ok(await repository.GetEmployeesAsync(employee.AccessToken, employee.BoardId));

        #endregion

        #region Profile

        [HttpPost]
        [Route(EventsKey.ChangeProfile)]
        public async Task<IActionResult> ChangeProfileAsync([FromBody] SetProfile profile) =>
            Ok(await repository.ChangeProfileAsync(profile.AccessToken, profile.FirstName, profile.LastName, profile.Bio,
                profile.Img, profile.Email, profile.PhoneNumber));

        [HttpPost]
        [Route(EventsKey.GetProfile)]
        public async Task<IActionResult> GetProfileInfoAsync([FromBody] GetProfile profile) =>
            Ok(await repository.GetProfileInfoAsync(profile.AccessToken));

        #endregion

        #region Notification

        [HttpPost]
        [Route(EventsKey.GetSystemMessages)]
        public async Task<IActionResult> GetSystemMessagesAsync([FromBody] GetMessages message) =>
            Ok(await repository.GetSystemMessagesAsync(message.AccessToken));

        [HttpPost]
        [Route(EventsKey.GetRecentChanges)]
        public async Task<IActionResult> GetRecentChangesAsync([FromBody] GetRecentChanges changes) =>
            Ok(await repository.GetRecentChangesAsync(changes.AccessToken));
        
        #endregion
    }
}
