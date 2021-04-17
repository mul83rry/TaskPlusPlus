using System;
using System.Threading.Tasks;

namespace TaskPlusPlus.API.Services
{
    public interface ITaskPlusPlusRepository
    {
        Task<Tuple<bool, string>> SignupAsync(string firstName, string lastName, string phoneNumber);
        Task<Tuple<bool, string>> SigninAsync(string phoneNumber);

        Task<string> GetBoardsAsync(string accessToken);
        Task<bool> AddBoardAsync(string accessToken, string caption);
        Task<bool> UpdateBoardAsync(string accessToken, Guid boardId);
        Task<bool> DeleteBoardAsync(string accessToken, Guid boardId);

        Task<string> GetTasksAsync(string accessToken, Guid parentId);
        Task<bool> AddTaskAsync(string accessToken, Guid parentId, string caption);
        Task<bool> EditTaskAsync(string accessToken, Guid parentId, string caption);
        Task<bool> AddSubTaskAsync(string accessToken, Guid parentId, string caption);
        Task<bool> EditSubTaskAsync(string accessToken, Guid parentId, string caption);


    }
}
