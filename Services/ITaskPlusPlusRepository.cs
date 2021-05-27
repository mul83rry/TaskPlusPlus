using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace TaskPlusPlus.API.Services
{
    public interface ITaskPlusPlusRepository
    {
        Task<JObject> SignupAsync(string firstName, string lastName, string phoneNumber);
        Task<JObject> SigninAsync(string phoneNumber);
        
        Task<string> GetBoardsAsync(string accessToken);
        Task<JObject> AddBoardAsync(string accessToken, string caption);
        Task<JObject> UpdateBoardAsync(string accessToken, Guid boardId, string caption);
        Task<JObject> DeleteBoardAsync(string accessToken, Guid boardId);

        Task<string> GetTasksAsync(string accessToken, Guid parentId);
        Task<JObject> AddTaskAsync(string accessToken, Guid parentId, string caption);
        Task<JObject> EditTaskAsync(string accessToken, Guid parentId, string caption, bool star);
        Task<JObject> AddSubTaskAsync(string accessToken, Guid parentId, string caption);
        Task<JObject> EditSubTaskAsync(string accessToken, Guid parentId, string caption, bool star);


    }
}
