using TaskPlusPlus.API.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaskPlusPlus.API.DbContexts;

namespace TaskPlusPlus.API.Services
{
    public partial class TaskPlusPlusRepository
    {
        public async Task<string> GetSystemMessagesAsync(string accessToken)
        {
            
            var user = await GetUserSessionAsync(accessToken);

            var res = from msg in context.Messages.Where(m => !m.Deleted && m.UserId == user.UserId).OrderBy(m => m.CreationAt)
                      select new
                      {
                          msg.Id,
                          msg.Content,
                          msg.CreationAt
                      };

            var JsonData = new JArray();

            foreach(var item in res)
            {
                (await context.Messages.SingleAsync(m => m.Id == item.Id)).Seen = true;
                JsonData.Add(new JObject()
                {
                    {"Id", item.Id},
                    {"CreationAt", item.CreationAt},
                    {"Content", item.Content}
                });
            }

            await context.SaveChangesAsync();

            return JsonData.ToString();
        }
    }
}
