using System;
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
        private ITaskPlusPlusRepository _taskPlusPlusRepository;

        public TaskPlusPlus(ITaskPlusPlusRepository taskPlusPlusRepository)
        {
            _taskPlusPlusRepository = taskPlusPlusRepository;
        }


        [HttpGet]
        public IActionResult Welcome()
        {
            return Ok("welcome to task++");
        }

        [HttpPost]
        [Route("test")]
        public IActionResult Test()
        {
            return Ok("this is ok");
        }

        [HttpGet("addfakedata")]
        public async Task<IActionResult> AddFakeDataAsync()
        {
            await _taskPlusPlusRepository.AddFakeData();

            return Ok("Done");
        }

        [HttpPost]
        [Route("signin")]
        public async Task<IActionResult> SigninAsync([FromBody] SignIn signIn)
        {
            var data = await _taskPlusPlusRepository.SigninAsync(signIn.PhoneNumber, signIn.OsVersion, signIn.DeviceType, signIn.BrowerVersion, signIn.Orientation);
            return Ok(data.ToString());
        }

        [HttpPost]
        [Route("addboard")]
        public async Task<IActionResult> AddBoardAsync([FromBody] AddBoard board)
        {
            var data = await _taskPlusPlusRepository.AddBoardAsync(board.AccessToken, board.Caption);
            return Ok(data.ToString());
        }

        [HttpPost]
        [Route("editboard")]
        public async Task<IActionResult> UpdateBoardAsync([FromBody] EditBoard newBoard)
        {
            var data = await _taskPlusPlusRepository.UpdateBoardAsync(newBoard.AccessToken, newBoard.Id, newBoard.Caption);
            return Ok(data.ToString());
        }

        [HttpPost]
        [Route("deleteboard")]
        public async Task<IActionResult> DeleteBoardAsync([FromBody] DeleteBoard board)
        {
            var data = await _taskPlusPlusRepository.DeleteBoardAsync(board.AccessToken, board.Id);
            return Ok(data.ToString());
        }
        [HttpPost]
        [Route("getboards")]
        public async Task<IActionResult> GetBoardsListAsync([FromBody] GetBoard accessToken)
        {
            var data = await _taskPlusPlusRepository.GetBoardsAsync(accessToken.AccessToken);
            return Ok(data);
        }

        [HttpPost]
        [Route("gettasks")]
        public async Task<IActionResult> GetTaskAsync([FromBody] GetTask task)
        {
            var data = await _taskPlusPlusRepository.GetTasksAsync(task.AccessToken, task.ParentId);
            return Ok(data);
        }

        [HttpPost]
        [Route("addtask")]
        public async Task<IActionResult> AddTaskAsync([FromBody] AddTask task)
        {
            var data = await _taskPlusPlusRepository.AddTaskAsync(task.AccessToken, task.ParentId, task.Caption);
            return Ok(data.ToString());
        }

        [HttpPost]
        [Route("edittask")]
        public async Task<IActionResult> EditTaskAsync([FromBody] EditTask task)
        {
            var data = await _taskPlusPlusRepository.EditTaskAsync(task.AccessToken, task.Id, task.Caption, task.Star);
            return Ok(data.ToString());
        }

        [HttpPost]
        [Route("addsubtask")]
        public async Task<IActionResult> AddSubTaskAsync([FromBody] AddTask task)
        {
            var data = await _taskPlusPlusRepository.AddSubTaskAsync(task.AccessToken, task.ParentId, task.Caption);
            return Ok(data.ToString());
        }
        [HttpPost]
        [Route("editSubtask")]
        public async Task<IActionResult> EditSubTaskAsync([FromBody] EditTask task)
        {
            var data = await _taskPlusPlusRepository.EditSubTaskAsync(task.AccessToken, task.Id, task.Caption, task.Star);
            return Ok(data.ToString());
        }

        [HttpPost]
        [Route("compeletetask")]
        public async Task<IActionResult> CompeletTaskAsync([FromBody] CompeleteTask task)
        {
            var data = await _taskPlusPlusRepository.CompeleteTaskAsync(task.AccessToken, task.Id);
            return Ok(data.ToString());
        }

        [HttpPost]
        [Route("deletetask")]

        public async Task<IActionResult> DeleteTaskAsync([FromBody] DeleteTask task)
        {
            var data = await _taskPlusPlusRepository.DeleteTaskAsync(task.AccessToken, task.Id);
            return Ok(data.ToString());
        }


        [HttpPost]
        [Route("addcomment")]

        public async Task<IActionResult> AddCommentAsync([FromBody] AddComment comment)
        {
            var data = await _taskPlusPlusRepository.AddCommentAsync(comment.AccessToken, comment.Content, comment.ParentId, comment.ReplyTo);
            return Ok(data.ToString());
        }


        [HttpPost]
        [Route("getcomments")]

        public async Task<IActionResult> GetCommentsAsync([FromBody] GetComment comment)
        {
            var data = await _taskPlusPlusRepository.GetCommentsAsync(comment.AccessToken, comment.ParentId);
            return Ok(data.ToString());
        }


        [HttpPost]
        [Route("editcomment")]

        public async Task<IActionResult> EditCommentAsync([FromBody] EditComment comment)
        {
            var data = await _taskPlusPlusRepository.EditCommentAsync(comment.AccessToken, comment.ParentId, comment.Id, comment.Content);
            return Ok(data.ToString());
        }


        [HttpPost]
        [Route("deletecomment")]

        public async Task<IActionResult> DeleteCommentAsync([FromBody] DeleteComment comment)
        {
            var data = await _taskPlusPlusRepository.DeleteCommentAsync(comment.AccessToken, comment.ParentId, comment.Id);
            return Ok(data.ToString());
        }


        [HttpPost]
        [Route("addfriend")]

        public async Task<IActionResult> AddFriendAsync([FromBody] AddFriend friend)
        {
            var data = await _taskPlusPlusRepository.AddFriendAsync(friend.AccessToken, friend.PhoneNumber);
            return Ok(data.ToString());
        }


        [HttpPost]
        [Route("getfriends")]

        public async Task<IActionResult> GetFriendListAsync([FromBody] GetFriendList friend)
        {
            var data = await _taskPlusPlusRepository.GetFriendsListAsync(friend.AccessToken);
            return Ok(data.ToString());
        }

        [HttpPost]
        [Route("getfriendrequests")]

        public async Task<IActionResult> GetFriendsRequestQueueAsync([FromBody] GetFriendRequest friend)
        {
            var data = await _taskPlusPlusRepository.GetFriendRequestQueueAsync(friend.AccessToken);
            return Ok(data.ToString());
        }


        [HttpPost]
        [Route("applyrequestresponce")]

        public async Task<IActionResult> ApplyFriendRequestResponceAsync([FromBody] RequestResponce request)
        {
            var data = await _taskPlusPlusRepository.ApplyFriendRequestResponceAsync(request.AccessToken, request.Id, request.Responce);
            return Ok(data.ToString());
        }


        [HttpPost]
        [Route("removefriend")]

        public async Task<IActionResult> RemoveFriendAsync([FromBody] RemoveFriend request)
        {
            var data = await _taskPlusPlusRepository.RemoveFriendAsync(request.AccessToken, request.Id);
            return Ok(data.ToString());
        }

        [HttpPost]
        [Route("shareboard")]

        public async Task<IActionResult> ShareBoardAsync([FromBody] ShareBoard share)
        {
            var data = await _taskPlusPlusRepository.ShareBoardAsync(share.AccessToken, share.BoardId, share.ShareToList);
            return Ok(data.ToString());
        }



        [HttpPost]
        [Route("addtag")]

        public async Task<IActionResult> AddTagAsync([FromBody] AddTag tag)
        {
            var data = await _taskPlusPlusRepository.AddTagAsync(tag.AccessToken, tag.BoardId, tag.Caption);
            return Ok(data.ToString());
        }


        [HttpPost]
        [Route("getboardstag")]

        public async Task<IActionResult> GetTagListAsync([FromBody] GetTag tag)
        {
            var data = await _taskPlusPlusRepository.GetTagListAsync(tag.AccessToken, tag.BoardId);
            return Ok(data.ToString());
        }

        [HttpPost]
        [Route("removetag")]

        public async Task<IActionResult> RemoveTagAsync([FromBody] RemoveTag tag)
        {
            var data = await _taskPlusPlusRepository.RemoveTagAsync(tag.AccessToken, tag.BoardId, tag.Id);
            return Ok(data.ToString());
        }

        [HttpPost]
        [Route("asigntag")]

        public async Task<IActionResult> AsignTagToTaskAsync([FromBody] AsignTag tag)
        {
            var data = await _taskPlusPlusRepository.AsignTagToTaskAsync(tag.AccessToken, tag.TaskId, tag.Id);
            return Ok(data.ToString());
        }


        [HttpPost]
        [Route("removeassignment")]

        public async Task<IActionResult> RemoveTagFromTaskAsync([FromBody] RemoveAssignment tag)
        {
            var data = await _taskPlusPlusRepository.RemoveTagFromTaskAsync(tag.AccessToken, tag.TaskId, tag.TaskTagId);
            return Ok(data.ToString());
        }


        [HttpPost]
        [Route("addrole")]

        public async Task<IActionResult> AddRoleAsync([FromBody] AddRole role)
        {
            var data = await _taskPlusPlusRepository.AddRoleAsync(role.AccessToken, role.BoardId, role.Caption, role.Color, role.ReadTask, role.WriteTask, role.CompleteTask, role.ReadComment, role.WriteComment);
            return Ok(data.ToString());
        }

        [HttpPost]
        [Route("asigntagtorole")]

        public async Task<IActionResult> AsignTagToRoleAsync([FromBody] AsignTagRole role)
        {
            var data = await _taskPlusPlusRepository.AsignTagToRoleAsync(role.AccessToken, role.BoardId, role.RoleId, role.TagId);
            return Ok(data.ToString());
        }

        [HttpPost]
        [Route("removetagfromrole")]

        public async Task<IActionResult> RemoveTagFromRoleAsync([FromBody] RemoveTagRole role)
        {
            var data = await _taskPlusPlusRepository.RemoveTagFromRoleAsync(role.AccessToken, role.BoardId, role.RoleTagId);
            return Ok(data.ToString());
        }


        [HttpPost]
        [Route("editrole")]

        public async Task<IActionResult> EditRoleAsync([FromBody] EditRole role)
        {
            var data = await _taskPlusPlusRepository.EditRoleAsync(role.AccessToken, role.Id, role.BoardId, role.Color, role.ReadTask, role.WriteTask, role.CompleteTask, role.ReadComment, role.WriteComment);
            return Ok(data.ToString());
        }


        [HttpPost]
        [Route("getboardroles")]

        public async Task<IActionResult> GetBoardRolesAsync([FromBody] GetRole role)
        {
            var data = await _taskPlusPlusRepository.GetBoardRolesAsync(role.AccessToken, role.BoardId);
            return Ok(data.ToString());
        }


        [HttpPost]
        [Route("asignrole")]

        public async Task<IActionResult> AsignRoleToEmployeesAsync([FromBody] AsignRole role)
        {
            var data = await _taskPlusPlusRepository.AsignRoleToEmployeesAsync(role.AccessToken, role.BoardId, role.RoleId, role.EmployeeId);
            return Ok(data.ToString());
        }

        [HttpPost]
        [Route("demote")]

        public async Task<IActionResult> DemoteEmployeesAsync([FromBody] Demote role)
        {
            var data = await _taskPlusPlusRepository.DemoteEmployeesRoleAsync(role.AccessToken, role.BoardId, role.RoleSessionId);
            return Ok(data.ToString());
        }

        [HttpPost]
        [Route("removerole")]

        public async Task<IActionResult> RemoveRoleFromBoardAsync([FromBody] RemoveRole role)
        {
            var data = await _taskPlusPlusRepository.RemoveRoleFromBoardAsync(role.AccessToken, role.BoardId, role.Id);
            return Ok(data.ToString());
        }

        [HttpPost]
        [Route("getemployees")]

        public async Task<IActionResult> GetEmployeesAsync([FromBody] GetEmployee employee)
        {
            var data = await _taskPlusPlusRepository.GetEmployeesAsync(employee.AccessToken, employee.BoardId);
            return Ok(data.ToString());
        }

        [HttpPost]
        [Route("edittag")]

        public async Task<IActionResult> EditTagAsync([FromBody] EditTag tag)
        {
            var data = await _taskPlusPlusRepository.EditTagAsync(tag.AccessToken, tag.BoardId, tag.Id, tag.Color);
            return Ok(data.ToString());
        }

        [HttpPost]
        [Route("removeshare")]

        public async Task<IActionResult> RemoveBoardShareAsync([FromBody] RemoveEmployee employee)
        {
            var data = await _taskPlusPlusRepository.RemoveBoardShareAsync(employee.AccessToken, employee.BoardId, employee.ShareId);
            return Ok(data.ToString());
        }


        [HttpPost]
        [Route("changeprofile")]

        public async Task<IActionResult> ChangeProfileAsync([FromBody] SetProfile profile)
        {
            var data = await _taskPlusPlusRepository.ChangeProfileAsync(profile.AccessToken, profile.FirstName, profile.LastName, profile.Bio, profile.Img, profile.Email, profile.PhoneNumber);
            return Ok(data.ToString());
        }

        [HttpPost]
        [Route("getprofile")]
        public async Task<IActionResult> GetProfileInfoAsync([FromBody] GetProfile profile)
        {
            var data = await _taskPlusPlusRepository.GetProfileInfoAsync(profile.AccessToken);
            return Ok(data.ToString());
        }

        [HttpPost]
        [Route("getsystemmessages")]
        public async Task<IActionResult> GetSystemMessagesAsync([FromBody] GetMessages message)
        {
            var data = await _taskPlusPlusRepository.GetSystemMessagesAsync(message.AccessToken);
            return Ok(data.ToString());
        }

        [HttpPost]
        [Route("getrecentchanges")]
        public async Task<IActionResult> GetRecentChangesAsync([FromBody] GetRecentChanges changes)
        {
            var data = await _taskPlusPlusRepository.GetRecentChangesAsync(changes.AccessToken);
            return Ok(data.ToString());
        }
    }
}
