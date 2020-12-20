using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Blog.Models
{
    public class PostModel
    {
        [Key]
        [Required]
        public int ID { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public DateTime DateTime { get; set; }
        [Required]
        public string Body { get; set; }

        public List<TagModel> Tags { get; set; }
        public List<CommentModel> Comments { get; set; }
        public List<LikeModel> Likes { get; set; }
    }
}