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
            return Ok("welcome");
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

    }
}
