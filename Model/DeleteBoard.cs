﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskPlusPlus.API.Model
{
    public class DeleteBoard
    {
        public Guid Id { get; set; }
        public string AccessToken { get; set; }
    }
}