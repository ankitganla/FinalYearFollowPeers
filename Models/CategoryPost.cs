using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Web;

namespace FollowPeers.Models
{
    public class CategoryPost
    {
        [Key]
        public int CategoryPostId { get; set; }
        public int Type { get; set; }
        public string PostMessage { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Category { get; set; }
        public string Link { get; set; }
        public int Postedby { get; set; }
    }
}