using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TaskPlusPlus.API.Models;
using TaskPlusPlus.API.Models.Task;
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
        public async Task<IActionResult> AddBoardAsync([FromBody] BoardModel board)
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
        public async Task<IActionResult> GetBoardsListAsync([FromBody] AccessCode accessToken)
        {
            var data = await _taskPlusPlusRepository.GetBoardsAsync(accessToken.AccessToken);
            return Ok(data);
        }

        [HttpPost]
        [Route("gettasks")]
        public async Task<IActionResult> GetTaskAsync([FromBody] TaskPlusPlus.API.Models.Task.Task task)
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

        [HttpGet]
        [Route("task/compelete/{accessToken}/{parentId:guid}")]
        public async Task<IActionResult> CompeletTaskAsync(string accessToken, Guid parentId)
        {
            var data = await _taskPlusPlusRepository.CompeleteTaskAsync(accessToken, parentId);
            return Ok(data.ToString());
        }

        [HttpPost]
        [Route("deletetask")]

        public async Task<IActionResult> DeleteTaskAsync([FromBody] DeleteTask task)
        {
            var data = await _taskPlusPlusRepository.DeleteTaskAsync(task.AccessToken, task.Id );
            return Ok(data.ToString());
        }


        [HttpGet]
        [Route("comment/add/{accessToken}/{parentId:guid}/{text}")]

        public async Task<IActionResult> AddCommentAsync(string accessToken, Guid parentId, string text)
        {
            var data = await _taskPlusPlusRepository.AddCommentAsync(accessToken, parentId, text);
            return Ok(data.ToString());
        }


        [HttpGet]
        [Route("comment/get/{accessToken}/{parentId:guid}")]

        public async Task<IActionResult> GetCommentsAsync(string accessToken, Guid parentId)
        {
            var data = await _taskPlusPlusRepository.GetCommentsAsync(accessToken, parentId);
            return Ok(data.ToString());
        }


        [HttpGet]
        [Route("comment/edit/{accessToken}/{parentId:guid}/{commentId:guid}/{text}")]

        public async Task<IActionResult> EditCommentAsync(string accessToken,Guid parentId, Guid commentId, string text)
        {
            var data = await _taskPlusPlusRepository.EditCommentAsync(accessToken, parentId, commentId, text);
            return Ok(data.ToString());
        }


        [HttpGet]
        [Route("comment/delete/{accessToken}/{parentId:guid}/{commentId:guid}")]
        
        public async Task<IActionResult> DeleteCommentAsync(string accessToken,Guid parentId,Guid commentId)
        {
            var data = await _taskPlusPlusRepository.DeleteCommentAsync(accessToken, parentId, commentId);
            return Ok(data.ToString());
        }


        [HttpGet]
        [Route("friend/add/{accessToken}/{phoneNumber}")]

        public async Task<IActionResult> AddFriendAsync(string accessToken, string phoneNumber)
        {
            var data = await _taskPlusPlusRepository.AddFriendAsync(accessToken, phoneNumber);
            return Ok(data.ToString());
        }


        [HttpGet]
        [Route("friend/get/{accessToken}")]

        public async Task<IActionResult> GetFriendListAsync(string accessToken)
        {
            var data = await _taskPlusPlusRepository.GetFriendsListAsync(accessToken);
            return Ok(data.ToString());
        }

        [HttpGet]
        [Route("friend/requests/get/{accessToken}")]

        public async Task<IActionResult> GetFriendsRequestQueueAsync(string accessToken)
        {
            var data = await _taskPlusPlusRepository.GetFriendRequestQueueAsync(accessToken);
            return Ok(data.ToString());
        }


        [HttpGet]
        [Route("friend/apply/{accessToken}/{requestId:guid}/{reply:bool}")]

        public async Task<IActionResult> ApplyFriendRequestAsync(string accessToken, Guid requestId,bool reply)
        {
            var data = await _taskPlusPlusRepository.ApplyFriendRequestAsync(accessToken,requestId,reply);
            return Ok(data.ToString());
        }


        [HttpGet]
        [Route("friend/remove/{accessToken}/{requestId:guid}")]

        public async Task<IActionResult> RemoveFriendAsync(string accessToken, Guid requestId)
        {
            var data = await _taskPlusPlusRepository.RemoveFriendAsync(accessToken, requestId);
            return Ok(data.ToString());
        }

        [HttpGet]
        [Route("board/share/{accessToken}/{boardId:guid}/{shareToList}")]

        public async Task<IActionResult> ShareBoardAsync(string accessToken,Guid boardId,string shareToList)
        {
            var data = await _taskPlusPlusRepository.ShareBoardAsync(accessToken, boardId, shareToList);
            return Ok(data.ToString());
        }



        [HttpGet]
        [Route("board/tag/add/{accessToken}/{boardId:guid}/{caption}")]


        public async Task<IActionResult> AddTagAsync(string accessToken,Guid boardId,string caption)
        {
            var data = await _taskPlusPlusRepository.AddTagAsync(accessToken, boardId, caption);
            return Ok(data.ToString());
        }


        [HttpGet]
        [Route("board/tag/get/{accessToken}/{parentId:guid}")]

        public async Task<IActionResult> GetTagListAsync(string accessToken, Guid parentId)
        {
            var data = await _taskPlusPlusRepository.GetTagListAsync(accessToken, parentId);
            return Ok(data.ToString());
        }


        [HttpGet]
        [Route("task/tag/add/{accessToken}/{taskId:guid}/{tagId:guid}")]

        public async Task<IActionResult> AsignTagToTaskAsync(string accessToken,Guid taskId,Guid tagId)
        {
            var data = await _taskPlusPlusRepository.AsignTagToTaskAsync(accessToken, taskId, tagId);
            return Ok(data.ToString());
        }


        [HttpGet]
        [Route("task/tag/remove/{accessToken}/{taskId:guid}/{taskTagId:guid}")]

        public async Task<IActionResult> RemoveTagFromTaskAsync(string accessToken, Guid taskId, Guid taskTagId)
        {
            var data = await _taskPlusPlusRepository.RemoveTagFromTaskAsync(accessToken, taskId, taskTagId);
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
        [Route("board/tag/remove/{accessToken}/{boardId:guid}/{tagId:guid}")]

        public async Task<IActionResult> RemoveTagAsync(string accessToken, Guid boardId, Guid tagId)
        {
            var data = await _taskPlusPlusRepository.RemoveTagAsync(accessToken, boardId, tagId);
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
