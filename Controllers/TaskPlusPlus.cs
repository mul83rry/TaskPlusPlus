using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
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
        [Route("signin/{phonenumber}")]
        public async Task<IActionResult> Signin(string phoneNumber)
        {
            var data = await _taskPlusPlusRepository.SigninAsync(phoneNumber);
            return Ok(data.ToString());
        }
        [HttpGet]
        [Route("signup/{fname}/{lname}/{pnumber}")]
        public async Task<IActionResult> Signup(string fName, string lName, string pNumber)
        {
            var data = await _taskPlusPlusRepository.SignupAsync(fName, lName, pNumber);
            return Ok(data.ToString());
        }

        [HttpGet]
        [Route("board/add/{accessToken}/{caption}")]
        public async Task<IActionResult> AddBoard(string accessToken, string caption)
        {
            var data = await _taskPlusPlusRepository.AddBoardAsync(accessToken, caption);
            return Ok(data.ToString());
        }
        [HttpGet]
        [Route("board/update/{accessToken}/{boardId}/{caption}")]
        public async Task<IActionResult> UpdateBoard(string accessToken, Guid boardId, string caption)
        {
            var data = await _taskPlusPlusRepository.UpdateBoardAsync(accessToken, boardId, caption);
            return Ok(data.ToString());
        }
        [HttpGet]
        [Route("board/delete/{accessToken}/{boardId}")]
        public async Task<IActionResult> DeleteBoard(string accessToken, Guid boardId)
        {
            var data = await _taskPlusPlusRepository.DeleteBoardAsync(accessToken, boardId);
            return Ok(data.ToString());
        }
        [HttpGet]
        [Route("board/list/{accessToken}")]
        public async Task<IActionResult> GetBoardsList(string accessToken)
        {
            var data = await _taskPlusPlusRepository.GetBoardsAsync(accessToken);
            return Ok(data.ToString());
        }

        [HttpGet]
        [Route("task/list/{accessToken}/{parentId}")]
        public async Task<IActionResult> GetTask(string accessToken, Guid parentId)
        {
            var data = await _taskPlusPlusRepository.GetTasksAsync(accessToken, parentId);
            return Ok(data.ToString());
        }
        [HttpGet]
        [Route("task/add/{accessToken}/{parentId}/{caption}")]
        public async Task<IActionResult> AddTask(string accessToken, Guid parentId, string caption)
        {
            var data = await _taskPlusPlusRepository.AddTaskAsync(accessToken, parentId, caption);
            return Ok(data.ToString());
        }
        [HttpGet]
        [Route("task/edit/{accessToken}/{parentId}/{caption}/{star}")]
        public async Task<IActionResult> EditTask(string accessToken, Guid parentId, string caption, bool star)
        {
            var data = await _taskPlusPlusRepository.EditTaskAsync(accessToken, parentId, caption, star);
            return Ok(data.ToString());
        }
        [HttpGet]
        [Route("subtask/add/{accessToken}/{parentId}/{caption}")]
        public async Task<IActionResult> AddSubTask(string accessToken, Guid parentId, string caption)
        {
            var data = await _taskPlusPlusRepository.AddSubTaskAsync(accessToken, parentId, caption);
            return Ok(data.ToString());
        }
        [HttpGet]
        [Route("subtask/edit/{accessToken}/{parentId}/{caption}/{star}")]
        public async Task<IActionResult> EditSubTask(string accessToken, Guid parentId, string caption, bool star)
        {
            var data = await _taskPlusPlusRepository.EditSubTaskAsync(accessToken, parentId, caption, star);
            return Ok(data.ToString());
        }

    }
}
