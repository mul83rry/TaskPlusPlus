﻿using CourseLibrary.API.Entities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CourseLibrary.API.Services
{
    public interface ICourseLibraryRepository
    {
        Task<Tuple<bool, string>> Signup(string firstName, string lastName, string phoneNumber);

        IEnumerable<JObject> GetBoards(string accessToken);
        Task<bool> AddBoard(string accessToken, string caption);
        Task<bool> UpdateBoard(string accessToken, Guid boardId);
        Task<bool> DeleteBoard(string accessToken, Guid boardId);
        
        IEnumerable<JObject> GetTasks(string accessToken, Guid parentId);
        Task<bool> AddTask(string accessToken, Guid parentId, string caption);
        Task<bool> EditTask(string accessToken, Guid parentId, string caption);
        Task<bool> AddSubTask(string accessToken, Guid parentId, string caption);
        Task<bool> EditSubTask(string accessToken, Guid parentId, string caption);


    }
}
