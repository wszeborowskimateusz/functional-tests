﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Blog.DAL.Model
{
    public class Post
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public string Content { get; set; }

        public string Author { get; set; }

        public List<Comment> Comments { get; set; }
    }
}
