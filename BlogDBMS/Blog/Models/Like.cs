//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Blog.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Like
    {
        public int ID { get; set; }
        public int PostID { get; set; }
        public int UserID { get; set; }
        public System.DateTime DateTime { get; set; }
    
        public virtual tblUser tblUser { get; set; }
        public virtual Post Post1 { get; set; }
    }
}