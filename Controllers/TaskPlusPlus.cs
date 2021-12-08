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
            Ok((await repository.SigninAsync(signIn.PhoneNumber, signIn.OsVersion, signIn.DeviceType, signIn.BrowerVersion, signIn.Orientation)).ToString());
        
        
        [HttpPost]
        [Route("Signin0")]
        public async Task<IActionResult> Signin0() =>
            Ok("signin0");

        #endregion

        #region Board

        [HttpPost]
        [Route(EventsKey.AddBoard)]
        public async Task<IActionResult> AddBoardAsync([FromBody] AddBoard board) =>
            Ok((await repository.AddBoardAsync(board.AccessToken, board.Caption)).ToString());

        [HttpPost]
        [Route(EventsKey.EditBoard)]
        public async Task<IActionResult> UpdateBoardAsync([FromBody] EditBoard newBoard) =>
            Ok((await repository.UpdateBoardAsync(newBoard.AccessToken, newBoard.Id, newBoard.Caption)).ToString());

        [HttpPost]
        [Route(EventsKey.DeleteBoard)]
        public async Task<IActionResult> DeleteBoardAsync([FromBody] DeleteBoard board) =>
            Ok((await repository.DeleteBoardAsync(board.AccessToken, board.Id)).ToString());

        [HttpPost]
        [Route(EventsKey.GetBoards)]
        public async Task<IActionResult> GetBoardsListAsync([FromBody] GetBoard accessToken) =>
            Ok((await repository.GetBoardsAsync(accessToken.AccessToken)).ToString());

        [HttpPost]
        [Route(EventsKey.ShareBoard)]
        public async Task<IActionResult> ShareBoardAsync([FromBody] ShareBoard share) =>
            Ok((await repository.ShareBoardAsync(share.AccessToken, share.BoardId, share.ShareToList)).ToString());

        [HttpPost]
        [Route(EventsKey.RemoveShare)]
        public async Task<IActionResult> RemoveBoardShareAsync([FromBody] RemoveEmployee employee) =>
                    Ok((await repository.RemoveBoardShareAsync(employee.AccessToken, employee.BoardId, employee.ShareId)).ToString());

        #endregion

        #region Task

        [HttpPost]
        [Route(EventsKey.GetTasks)]
        public async Task<IActionResult> GetTaskAsync([FromBody] GetTask task) =>
            Ok((await repository.GetTasksAsync(task.AccessToken, task.ParentId)).ToString());

        [HttpPost]
        [Route(EventsKey.AddTask)]
        public async Task<IActionResult> AddTaskAsync([FromBody] AddTask task) =>
            Ok((await repository.AddTaskAsync(task.AccessToken, task.ParentId, task.Caption)).ToString());

        [HttpPost]
        [Route(EventsKey.EditTask)]
        public async Task<IActionResult> EditTaskAsync([FromBody] EditTask task) =>
            Ok((await repository.EditTaskAsync(task.AccessToken, task.Id, task.Caption, task.Star)).ToString());

        [HttpPost]
        [Route(EventsKey.AddsubTask)]
        public async Task<IActionResult> AddSubTaskAsync([FromBody] AddTask task) =>
            Ok((await repository.AddSubTaskAsync(task.AccessToken, task.ParentId, task.Caption)).ToString());

        [HttpPost]
        [Route(EventsKey.EditSubtask)]
        public async Task<IActionResult> EditSubTaskAsync([FromBody] EditTask task) =>
            Ok((await repository.EditSubTaskAsync(task.AccessToken, task.Id, task.Caption, task.Star)).ToString());

        [HttpPost]
        [Route(EventsKey.CompeleteTask)]
        public async Task<IActionResult> CompeletTaskAsync([FromBody] CompeleteTask task) =>
            Ok((await repository.CompeleteTaskAsync(task.AccessToken, task.Id)).ToString());

        [HttpPost]
        [Route(EventsKey.DeleteTask)]
        public async Task<IActionResult> DeleteTaskAsync([FromBody] DeleteTask task) =>
            Ok((await repository.DeleteTaskAsync(task.AccessToken, task.Id)).ToString());

        #endregion

        #region Comment

        [HttpPost]
        [Route(EventsKey.AddComment)]
        public async Task<IActionResult> AddCommentAsync([FromBody] AddComment comment) =>
            Ok((await repository.AddCommentAsync(comment.AccessToken, comment.Content, comment.ParentId, comment.ReplyTo)).ToString());


        [HttpPost]
        [Route(EventsKey.GetComments)]
        public async Task<IActionResult> GetCommentsAsync([FromBody] GetComment comment) =>
            Ok((await repository.GetCommentsAsync(comment.AccessToken, comment.ParentId)).ToString());


        [HttpPost]
        [Route(EventsKey.EditComment)]
        public async Task<IActionResult> EditCommentAsync([FromBody] EditComment comment) =>
            Ok((await repository.EditCommentAsync(comment.AccessToken, comment.ParentId, comment.Id, comment.Content)).ToString());


        [HttpPost]
        [Route(EventsKey.DeleteComment)]
        public async Task<IActionResult> DeleteCommentAsync([FromBody] DeleteComment comment) =>
            Ok((await repository.DeleteCommentAsync(comment.AccessToken, comment.ParentId, comment.Id)).ToString());

        #endregion

        #region Friend

        [HttpPost]
        [Route(EventsKey.AddFriend)]
        public async Task<IActionResult> AddFriendAsync([FromBody] AddFriend friend) =>
            Ok((await repository.AddFriendAsync(friend.AccessToken, friend.PhoneNumber)).ToString());

        [HttpPost]
        [Route(EventsKey.GetFriends)]
        public async Task<IActionResult> GetFriendListAsync([FromBody] GetFriendList friend) =>
            Ok((await repository.GetFriendsListAsync(friend.AccessToken)).ToString());

        [HttpPost]
        [Route(EventsKey.GetFriendRequests)]
        public async Task<IActionResult> GetFriendsRequestQueueAsync([FromBody] GetFriendRequest friend) =>
            Ok((await repository.GetFriendRequestQueueAsync(friend.AccessToken)).ToString());

        [HttpPost]
        [Route(EventsKey.ApplyRequestResponce)]
        public async Task<IActionResult> ApplyFriendRequestResponceAsync([FromBody] RequestResponce request) =>
            Ok((await repository.ApplyFriendRequestResponceAsync(request.AccessToken, request.Id, request.Responce)).ToString());

        [HttpPost]
        [Route(EventsKey.RemoveFriend)]
        public async Task<IActionResult> RemoveFriendAsync([FromBody] RemoveFriend request) =>
            Ok((await repository.RemoveFriendAsync(request.AccessToken, request.Id)).ToString());

        #endregion

        #region Tag

        [HttpPost]
        [Route(EventsKey.AddTag)]
        public async Task<IActionResult> AddTagAsync([FromBody] AddTag tag) =>
            Ok((await repository.AddTagAsync(tag.AccessToken, tag.BoardId, tag.Caption)).ToString());

        [HttpPost]
        [Route(EventsKey.GetBoardsTag)]
        public async Task<IActionResult> GetTagListAsync([FromBody] GetTag tag) =>
            Ok((await repository.GetTagListAsync(tag.AccessToken, tag.BoardId)).ToString());

        [HttpPost]
        [Route(EventsKey.RemoveTag)]
        public async Task<IActionResult> RemoveTagAsync([FromBody] RemoveTag tag) =>
            Ok((await repository.RemoveTagAsync(tag.AccessToken, tag.BoardId, tag.Id)).ToString());

        [HttpPost]
        [Route(EventsKey.AsignTag)]
        public async Task<IActionResult> AsignTagToTaskAsync([FromBody] AsignTag tag) =>
            Ok((await repository.AsignTagToTaskAsync(tag.AccessToken, tag.TaskId, tag.Id)).ToString());

        [HttpPost]
        [Route(EventsKey.RemoveAssignment)]
        public async Task<IActionResult> RemoveTagFromTaskAsync([FromBody] RemoveAssignment tag) =>
            Ok((await repository.RemoveTagFromTaskAsync(tag.AccessToken, tag.TaskId, tag.TaskTagId)).ToString());

        [HttpPost]
        [Route(EventsKey.EditTag)]
        public async Task<IActionResult> EditTagAsync([FromBody] EditTag tag) =>
            Ok((await repository.EditTagAsync(tag.AccessToken, tag.BoardId, tag.Id, tag.Color)).ToString());

        #endregion

        #region Role

        [HttpPost]
        [Route(EventsKey.AddRole)]
        public async Task<IActionResult> AddRoleAsync([FromBody] AddRole role) =>
            Ok((await repository.AddRoleAsync(role.AccessToken, role.BoardId, role.Caption, role.Color,
                role.ReadTask, role.WriteTask, role.CompleteTask, role.ReadComment, role.WriteComment)).ToString());

        [HttpPost]
        [Route(EventsKey.AsignTagtoRole)]
        public async Task<IActionResult> AsignTagToRoleAsync([FromBody] AsignTagRole role) =>
            Ok((await repository.AsignTagToRoleAsync(role.AccessToken, role.BoardId, role.RoleId, role.TagId)).ToString());

        [HttpPost]
        [Route(EventsKey.RemoveTagFromRole)]
        public async Task<IActionResult> RemoveTagFromRoleAsync([FromBody] RemoveTagRole role) =>
            Ok((await repository.RemoveTagFromRoleAsync(role.AccessToken, role.BoardId, role.RoleTagId)).ToString());

        [HttpPost]
        [Route(EventsKey.EditRole)]
        public async Task<IActionResult> EditRoleAsync([FromBody] EditRole role) =>
            Ok((await repository.EditRoleAsync(role.AccessToken, role.Id, role.BoardId, role.Color, role.ReadTask,
                role.WriteTask, role.CompleteTask, role.ReadComment, role.WriteComment)).ToString());

        [HttpPost]
        [Route(EventsKey.GetBoardRoles)]
        public async Task<IActionResult> GetBoardRolesAsync([FromBody] GetRole role) =>
            Ok((await repository.GetBoardRolesAsync(role.AccessToken, role.BoardId)).ToString());

        [HttpPost]
        [Route(EventsKey.AsignRole)]
        public async Task<IActionResult> AsignRoleToEmployeesAsync([FromBody] AsignRole role) =>
            Ok((await repository.AsignRoleToEmployeesAsync(role.AccessToken, role.BoardId, role.RoleId, role.EmployeeId)).ToString());

        [HttpPost]
        [Route(EventsKey.Demote)]
        public async Task<IActionResult> DemoteEmployeesAsync([FromBody] Demote role) =>
            Ok((await repository.DemoteEmployeesRoleAsync(role.AccessToken, role.BoardId, role.RoleSessionId)).ToString());

        [HttpPost]
        [Route(EventsKey.RemoveRole)]
        public async Task<IActionResult> RemoveRoleFromBoardAsync([FromBody] RemoveRole role) =>
            Ok((await repository.RemoveRoleFromBoardAsync(role.AccessToken, role.BoardId, role.Id)).ToString());

        [HttpPost]
        [Route(EventsKey.GetEmployees)]
        public async Task<IActionResult> GetEmployeesAsync([FromBody] GetEmployee employee) =>
            Ok((await repository.GetEmployeesAsync(employee.AccessToken, employee.BoardId)).ToString());

        #endregion

        #region Profile

        [HttpPost]
        [Route(EventsKey.ChangeProfile)]
        public async Task<IActionResult> ChangeProfileAsync([FromBody] SetProfile profile) =>
            Ok((await repository.ChangeProfileAsync(profile.AccessToken, profile.FirstName, profile.LastName, profile.Bio,
                profile.Img, profile.Email, profile.PhoneNumber)).ToString());

        [HttpPost]
        [Route(EventsKey.GetProfile)]
        public async Task<IActionResult> GetProfileInfoAsync([FromBody] GetProfile profile) =>
            Ok((await repository.GetProfileInfoAsync(profile.AccessToken)).ToString());

        #endregion

        #region Notification

        [HttpPost]
        [Route(EventsKey.GetSystemMessages)]
        public async Task<IActionResult> GetSystemMessagesAsync([FromBody] GetMessages message) =>
            Ok((await repository.GetSystemMessagesAsync(message.AccessToken)).ToString());

        [HttpPost]
        [Route(EventsKey.GetRecentChanges)]
        public async Task<IActionResult> GetRecentChangesAsync([FromBody] GetRecentChanges changes) =>
            Ok((await repository.GetRecentChangesAsync(changes.AccessToken)).ToString());

        // todo: temp
        [HttpGet]
        [Route("GenerateDatabase")]
        public async Task<IActionResult> GenerateDatabase() =>
            Ok((await repository.GenerateDBAsync()).ToString());

        #endregion

        #region Public
        [HttpPost]
        [Route(EventsKey.GetParentInformation)]
        public async Task<IActionResult> GetParentInformationAsync([FromBody] GetParentInformation parentInfo) =>
            Ok(await repository.GetParentInformationAsync(parentInfo.AccessToken,parentInfo.ParentId));
        #endregion
    }
}
