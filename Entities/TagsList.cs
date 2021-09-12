﻿using System;
using System.ComponentModel.DataAnnotations;

namespace TaskPlusPlus.API.Entities
{
    public class TagsList
    {
        [Key]
        public Guid Id { get; set; }
        public Guid TagId { get; set; }
        public Guid TaskId { get; set; }
        public bool Removed { get; set; }
        public DateTime AsignDate { get; set; }
    }
}
