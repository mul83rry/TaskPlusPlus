using TaskPlusPlus.API.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaskPlusPlus.API.DbContexts;

namespace TaskPlusPlus.API.Services
{
    public partial class TaskPlusPlusRepository : ITaskPlusPlusRepository, IDisposable
    {

        public async Task<JObject> ChangeProfileAsync(string accessToken, string firstName, string lastName, string bio, string img, string email, string phoneNumber)
        {
            using var context = new TaskPlusPlusContext();
            var user = await GetUserSessionAsync(accessToken);
            var profile = await context.Profiles.SingleAsync(p => p.UserId == user.UserId);

            if (firstName == string.Empty) firstName = "user";

            profile.FirstName = firstName;
            profile.LastName = lastName;
            profile.Bio = bio;
            profile.Image = img;

            var mail = await context.Login.SingleAsync(l => l.Id == user.UserId);

            mail.Email = email;

            await context.SaveChangesAsync();

            return JsonMap.TrueResult;
        }


        public async Task<string> GetProfileInfoAsync(string accessToken)
        {
            using var context = new TaskPlusPlusContext();
            var user = await GetUserSessionAsync(accessToken);

            var profile = await context.Profiles.SingleAsync(p => p.UserId == user.UserId);

            var email = (await context.Login.SingleAsync(l => l.Id == user.UserId)).Email;

            var JsonData = new JArray();

            var data = new JObject()
            {
                { "FirstName", profile.FirstName},
                { "LastName", profile.LastName},
                { "PhoneNumber", profile.PhoneNumber},
                { "Bio", profile.Bio},
                { "Email", email},
                { "Img", profile.Image},
            };

            return data.ToString();
        }
    }
}
