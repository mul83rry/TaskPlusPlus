using TaskPlusPlus.API.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace TaskPlusPlus.API.Services
{
    public partial class TaskPlusPlusRepository
    {
        private async Task<Session> GetUserSessionAsync(string accessToken) => string.IsNullOrEmpty(accessToken)
            ? throw new ArgumentNullException("not valid argument")
            : await context.Sessions.SingleAsync(s => s.AccessToken == accessToken);


        public async Task<JObject> SigninAsync(string phoneNumber, string osVersion, string deviceType, string browerVersion, string orientation)
        {
            if (!await context.Login.AnyAsync(u => u.PhoneNumber == phoneNumber))
                return JsonMap.FalseResultWithEmptyAccessToken;

            var user = await context.Login.SingleAsync(u => u.PhoneNumber == phoneNumber);

            // Add new session
            var newSession = new Session()
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

            await context.Sessions.AddAsync(newSession);
            await context.SaveChangesAsync();
            return JsonMap.GetSuccesfullAccessToken(newSession.AccessToken);
        }

        public async Task<JObject> SignUpAsync(string firstName, string lastName, string phoneNumber, string osVersion, string deviceType, string browerVersion, string orientation)
        {
            if (await context.Login.AnyAsync(u => u.PhoneNumber == phoneNumber))
                return JsonMap.FalseResultWithEmptyAccessToken;

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


        private async Task<Profile> GetUser(Guid userId) => await context.Profiles.SingleOrDefaultAsync(u => u.UserId == userId);
    }
}
