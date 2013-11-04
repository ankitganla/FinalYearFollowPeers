using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FollowPeers.Models
{
    public class Achievement
    {
        public int AchievementId { set; get; }
        public int UserProfileId { set; get; }
        public string Title { set; get; }
        public string Description { set; get; }
        public string startYear { set; get; }
        public string endYear { set; get; }
        public string Keyword {set; get;}
        public int like { set; get; }
        public string Link { set; get; }

        public virtual UserProfile UserProfile { get; set; }
    }
}