﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskPlusPlus.API.Entities
{
    public class Task
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string Caption { get; set; }

        [Required]
        public bool Star { get; set; }

        [Required]
        public Guid Creator { get; set; }

        [Required]
        public Guid LastModifiedBy { get; set; }

        public Guid ParentId { get; set; }

        [Required]
        public DateTime CreationAt { get; set; }

        [Required]
        public bool Deleted { get; set; }
    }
}
