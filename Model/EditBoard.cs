﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskPlusPlus.API.Model
{
    public class EditBoard
    {
        public Guid Id { get; set; }
        public string Caption { get; set; }
        public string AccessToken { get; set; }
    }
}