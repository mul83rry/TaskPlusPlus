using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskPlusPlus.API.Model
{
    public class EditTask
    {
        public string AccessToken { get; set; }
        public Guid Id { get; set; }
        public string Caption { get; set; }
        public bool Star { get; set; }

    }
}
