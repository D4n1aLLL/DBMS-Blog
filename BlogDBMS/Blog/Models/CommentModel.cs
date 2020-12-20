using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Blog.Models
{
    public class CommentModel
    {
        [Key]
        public int ID { get; set; }
        //public Post Post { get; set; }
        //[ForeignKey("Post")]
        //public int PostID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Body { get; set; }
        public DateTime DateTime { get; set; }
    }
}