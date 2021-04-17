using TaskPlusPlus.API.Entities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaskPlusPlus.API.Services
{
    public interface ICourseLibraryRepository
    {
        Task<Tuple<bool, string>> Signup(string firstName, string lastName, string phoneNumber);
        Task<Tuple<bool, string>> Signin(string phoneNumber);

        Task<string> GetBoards(string accessToken);
        Task<bool> AddBoard(string accessToken, string caption);
        Task<bool> UpdateBoard(string accessToken, Guid boardId);
        Task<bool> DeleteBoard(string accessToken, Guid boardId);

        Task<string> GetTasks(string accessToken, Guid parentId);
        Task<bool> AddTask(string accessToken, Guid parentId, string caption);
        Task<bool> EditTask(string accessToken, Guid parentId, string caption);
        Task<bool> AddSubTask(string accessToken, Guid parentId, string caption);
        Task<bool> EditSubTask(string accessToken, Guid parentId, string caption);


    }
}
