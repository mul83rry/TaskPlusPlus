using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TaskPlusPlus.API.Models;
using TaskPlusPlus.API.Models.Task;
using TaskPlusPlus.API.Models.Board;
using TaskPlusPlus.API.Models.Comment;
using TaskPlusPlus.API.Models.Friend;
using TaskPlusPlus.API.Models.Tag;
using TaskPlusPlus.API.Services;

namespace TaskPlusPlus.API.Controllers
{
    [Route("api")]
    [ApiController]
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
            var data = await _taskPlusPlusRepository.DeleteTaskAsync(task.AccessToken, task.Id );
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
            var data = await _taskPlusPlusRepository.ApplyFriendRequestResponceAsync(request.AccessToken,request.Id,request.Responce);
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


        [HttpGet]
        [Route("board/role/add/{accessToken}/{boardId:guid}/{caption}/{readTask:bool}/{writeTask:bool}/{readComment:bool}/{writeComment:bool}/{tagList}")]

        public async Task<IActionResult> AddRoleAsync(string accessToken, Guid boardId, string caption, bool readTask, bool writeTask, bool readComment, bool writeComment, string tagList)
        {
            var data = await _taskPlusPlusRepository.AddRoleAsync(accessToken, boardId, caption, readTask, writeTask, readComment, writeComment, tagList);
            return Ok(data.ToString());
        }


        [HttpGet]
        [Route("board/role/get/{accessToken}/{boardId:guid}")]

        public async Task<IActionResult> GetBoardRolesAsync(string accessToken, Guid boardId)
        {
            var data = await _taskPlusPlusRepository.GetBoardRolesAsync(accessToken, boardId);
            return Ok(data.ToString());
        }


        [HttpGet]
        [Route("board/role/user/add/{accessToken}/{boardId:guid}/{roleId:guid}/{employeesId:guid}")]

        public async Task<IActionResult> AsignRoleToEmployeesAsync(string accessToken, Guid boardId, Guid roleId,Guid employeesId)
        {
            var data = await _taskPlusPlusRepository.AsignRoleToEmployeesAsync(accessToken, boardId, roleId, employeesId);
            return Ok(data.ToString());
        }

        [HttpGet]
        [Route("board/role/user/demote/{accessToken}/{boardId:guid}/{roleSessionId:guid}")]

        public async Task<IActionResult> DemoteEmployeesAsync(string accessToken, Guid boardId, Guid roleSessionId)
        {
            var data = await _taskPlusPlusRepository.DemoteEmployeesRoleAsync(accessToken, boardId, roleSessionId);
            return Ok(data.ToString());
        }

        [HttpGet]
        [Route("board/role/remove/{accessToken}/{boardId:guid}/{roleId:guid}")]

        public async Task<IActionResult> RemoveRoleFromBoardAsync(string accessToken, Guid boardId, Guid roleId)
        {
            var data = await _taskPlusPlusRepository.RemoveRoleFromBoardAsync(accessToken, boardId, roleId);
            return Ok(data.ToString());
        }

        [HttpGet]
        [Route("board/role/user/get/{accessToken}/{boardId:guid}")]

        public async Task<IActionResult> GetEmployeesRolesAsync(string accessToken, Guid boardId)
        {
            var data = await _taskPlusPlusRepository.GetEmployeesRolesAsync(accessToken, boardId);
            return Ok(data.ToString());
        }


        [HttpGet]
        [Route("board/employees/get/{accessToken}/{boardId:guid}")]

        public async Task<IActionResult> GetEmployeesAsync(string accessToken, Guid boardId)
        {
            var data = await _taskPlusPlusRepository.GetEmployeesAsync(accessToken, boardId);
            return Ok(data.ToString());
        }
    }
}
