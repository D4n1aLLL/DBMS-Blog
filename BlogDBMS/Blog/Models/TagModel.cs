using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Blog.Models
{
    public class TagModel
    {
        [Key]
        [Required]
        public int ID { get; set; }

        public string Name { get; set; }
        //public List<PostModel> Posts { get; set; }
    }
}