using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskPlusPlus.API.Model
{
    public class DeleteTask
    {
        public string AccessToken { get; set; }
        public Guid Id { get; set; }
    }
}
