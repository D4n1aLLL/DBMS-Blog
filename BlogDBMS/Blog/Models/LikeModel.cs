using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Blog.Models
{
    public class LikeModel
    {
        public int ID { get; set; }
        public DateTime DateTime { get; set; }
        public int UserID { get; set; }
    }
}