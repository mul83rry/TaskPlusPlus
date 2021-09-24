using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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

        [HttpGet]
        [Route("signin/{phoneNumber}/{osVersion}/{deviceType}/{browerVersion}/{orientation}")]
        public async Task<IActionResult> SigninAsync(string phoneNumber, string osVersion, string deviceType, string browerVersion, string orientation)
        {
            var data = await _taskPlusPlusRepository.SigninAsync(phoneNumber, osVersion, deviceType, browerVersion, orientation);
            return Ok(data.ToString());
        }
        [HttpGet]
        [Route("signup/{fName}/{lname}/{pNumber}/{osVersion}/{deviceType}/{browerVersion}/{orientation}")]
        public async Task<IActionResult> SignUpAsync(string fName, string lName, string pNumber, string osVersion, string deviceType, string browerVersion, string orientation)
        {
            var data = await _taskPlusPlusRepository.SignUpAsync(fName, lName, pNumber, osVersion, deviceType, browerVersion, orientation);
            return Ok(data.ToString());
        }

        [HttpGet]
        [Route("board/add/{accessToken}/{caption}")]
        public async Task<IActionResult> AddBoardAsync(string accessToken, string caption)
        {
            var data = await _taskPlusPlusRepository.AddBoardAsync(accessToken, caption);
            return Ok(data.ToString());
        }
        [HttpGet]
        [Route("board/update/{accessToken}/{boardId:guid}/{caption}")]
        public async Task<IActionResult> UpdateBoardAsync(string accessToken, Guid boardId, string caption)
        {
            var data = await _taskPlusPlusRepository.UpdateBoardAsync(accessToken, boardId, caption);
            return Ok(data.ToString());
        }
        [HttpGet]
        [Route("board/delete/{accessToken}/{boardId}")]
        public async Task<IActionResult> DeleteBoardAsync(string accessToken, Guid boardId)
        {
            var data = await _taskPlusPlusRepository.DeleteBoardAsync(accessToken, boardId);
            return Ok(data.ToString());
        }
        [HttpGet]
        [Route("board/list/{accessToken}")]
        public async Task<IActionResult> GetBoardsListAsync(string accessToken)
        {
            var data = await _taskPlusPlusRepository.GetBoardsAsync(accessToken);
            return Ok(data);
        }

        [HttpGet]
        [Route("task/list/{accessToken}/{parentId:guid}")]
        public async Task<IActionResult> GetTaskAsync(string accessToken, Guid parentId)
        {
            var data = await _taskPlusPlusRepository.GetTasksAsync(accessToken, parentId);
            return Ok(data);
        }
        [HttpGet]
        [Route("task/add/{accessToken}/{parentId}/{caption}")]
        public async Task<IActionResult> AddTaskAsync(string accessToken, Guid parentId, string caption)
        {
            var data = await _taskPlusPlusRepository.AddTaskAsync(accessToken, parentId, caption);
            return Ok(data.ToString());
        }
        [HttpGet]
        [Route("task/edit/{accessToken}/{parentId:guid}/{caption}/{star:bool}")]
        public async Task<IActionResult> EditTaskAsync(string accessToken, Guid parentId, string caption, bool star)
        {
            var data = await _taskPlusPlusRepository.EditTaskAsync(accessToken, parentId, caption, star);
            return Ok(data.ToString());
        }
        [HttpGet]
        [Route("subtask/add/{accessToken}/{parentId:guid}/{caption}")]
        public async Task<IActionResult> AddSubTaskAsync(string accessToken, Guid parentId, string caption)
        {
            var data = await _taskPlusPlusRepository.AddSubTaskAsync(accessToken, parentId, caption);
            return Ok(data.ToString());
        }
        [HttpGet]
        [Route("subtask/edit/{accessToken}/{parentId:guid}/{caption}/{star:bool}")]
        public async Task<IActionResult> EditSubTaskAsync(string accessToken, Guid parentId, string caption, bool star)
        {
            var data = await _taskPlusPlusRepository.EditSubTaskAsync(accessToken, parentId, caption, star);
            return Ok(data.ToString());
        }

        [HttpGet]
        [Route("task/compelete/{accessToken}/{parentId:guid}")]
        public async Task<IActionResult> CompeletTaskAsync(string accessToken, Guid parentId)
        {
            var data = await _taskPlusPlusRepository.CompeleteTaskAsync(accessToken, parentId);
            return Ok(data.ToString());
        }

        [HttpGet]
        [Route("task/delete/{accessToken}/{parentId:guid}")]

        public async Task<IActionResult> DeleteTaskAsync(string accessToken, Guid parentId)
        {
            var data = await _taskPlusPlusRepository.DeleteTaskAsync(accessToken, parentId);
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
