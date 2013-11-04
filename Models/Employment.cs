using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FollowPeers.Models
{
    public class Employment
    {
        public int EmploymentId { set; get; }
        public int UserProfileId { set; get; }
        public string EmployerName { set; get; }
        public int startYear { set; get; }
        public int endYear { set; get; }
   
        public string Description { set; get; }
        public string Role { set; get; }
        public virtual UserProfile UserProfile { get; set; }
    }
}