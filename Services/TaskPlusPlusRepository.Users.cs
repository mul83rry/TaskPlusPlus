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


        public async Task<JObject> SigninAsync(string phoneNumber)
        {
            if (!await context.Users.AnyAsync(u => u.PhoneNumber == phoneNumber))
                return JsonMap.FalseResultWithEmptyAccessToken;

            var user = await context.Users.SingleAsync(u => u.PhoneNumber == phoneNumber);

            // Add new session
            var newSession = new Session()
            {
                AccessToken = Guid.NewGuid().ToString(),
                Id = Guid.NewGuid(),
                IsValid = true,
                UserId = user.Id,
                CreationAt = DateTime.Now,
                LastFetchTime = DateTime.Now - TimeSpan.FromHours(1)
            };

            await context.Sessions.AddAsync(newSession);
            await context.SaveChangesAsync();
            return JsonMap.GetSuccesfullAccessToken(newSession.AccessToken);
        }

        public async Task<JObject> SignUpAsync(string firstName, string lastName, string phoneNumber)
        {
            if (await context.Users.AnyAsync(u => u.PhoneNumber == phoneNumber))
                return JsonMap.FalseResultWithEmptyAccessToken;

            var newUser = new User()
            {
                Id = Guid.NewGuid(),
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber,
                SignupDate = DateTime.Now
            };
            await context.Users.AddAsync(newUser);

            // Add new session
            var newSession = new Session()
            {
                AccessToken = Guid.NewGuid().ToString(),
                Id = Guid.NewGuid(),
                IsValid = true,
                UserId = newUser.Id,
                CreationAt = DateTime.Now
            };

            await context.Sessions.AddAsync(newSession);
            await context.SaveChangesAsync();
            return JsonMap.GetSuccesfullAccessToken(newSession.AccessToken);
        }


        private async Task<User> GetUser(Guid userId) => await context.Users.SingleOrDefaultAsync(u => u.Id == userId);
    }
}
