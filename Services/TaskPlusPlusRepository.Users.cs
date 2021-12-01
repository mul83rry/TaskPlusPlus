using TaskPlusPlus.API.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using TaskPlusPlus.API.Extensions;
using TaskPlusPlus.API.DbContexts;

namespace TaskPlusPlus.API.Services
{
    public partial class TaskPlusPlusRepository
    {
        private static async Task<Session> GetUserSessionAsync(string accessToken)
        {
            using var context = new TaskPlusPlusContext();
            return string.IsNullOrEmpty(accessToken)
                ? throw new ArgumentNullException("not valid argument")
                : await context.Sessions.SingleAsync(s => s.AccessToken == accessToken);
        }

        public async Task<JObject> SigninAsync(string phoneNumber, string osVersion, string deviceType, string browerVersion, string orientation)
        {
            if (!phoneNumber.IsValidPhoneNumber()) return JsonMap.FalseResult;

            using var context = new TaskPlusPlusContext();
            var userExist = await context.Login.AnyAsync(u => u.PhoneNumber == phoneNumber);

            if (userExist)
            {
                var user = await context.Login.SingleAsync(u => u.PhoneNumber == phoneNumber);
                // Add new session
                var session = new Session()
                {
                    AccessToken = Guid.NewGuid().ToString(),
                    Id = Guid.NewGuid(),
                    IsValid = true,
                    UserId = user.Id,
                    CreationAt = DateTime.Now,
                    OsVersion = osVersion,
                    DeviceType = deviceType,
                    BrowerVersion = browerVersion,
                    Orientation = orientation,
                    LastFetchTime = DateTime.Now - TimeSpan.FromHours(1)
                };

                await context.Sessions.AddAsync(session);
                await context.SaveChangesAsync();

                return JsonMap.GetSuccesfullAccessToken(session.AccessToken);
            }

            // add new user
            var newUser = new Login()
            {
                Id = Guid.NewGuid(),
                PhoneNumber = phoneNumber,
                Email = string.Empty
            };

            await context.Login.AddAsync(newUser);

            // Add new session
            var newSession = new Session()
            {
                AccessToken = Guid.NewGuid().ToString(),
                Id = Guid.NewGuid(),
                IsValid = true,
                UserId = newUser.Id,
                OsVersion = osVersion,
                DeviceType = deviceType,
                BrowerVersion = browerVersion,
                Orientation = orientation,
                CreationAt = DateTime.Now
            };

            await context.Sessions.AddAsync(newSession);

            // add new profile row
            var newProfile = new Profile()
            {
                Id = Guid.NewGuid(),
                UserId = newUser.Id,
                Bio = string.Empty,
                Image = string.Empty,
                PhoneNumber = newUser.PhoneNumber,
                SignupDate = DateTime.Now,
                FirstName = "User",
                LastName = string.Empty
            };

            await context.Profiles.AddAsync(newProfile);
            await context.SaveChangesAsync();

            return JsonMap.GetSuccesfullAccessToken(newSession.AccessToken);
        }

        private static async Task<Profile> GetUser(Guid userId)
        {
            using var context = new TaskPlusPlusContext();
            return await context.Profiles.SingleOrDefaultAsync(u => u.UserId == userId);
        }
    }
}
