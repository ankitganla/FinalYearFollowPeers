using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace FollowPeers.Models
{
    public class AchievementLike
    {   
        [Key]
        public int AchievementlikeId { set; get; }
        public int AchievementId { set; get; }
        public int UserProfileId { set; get; }
        public int Type { set; get; }

        public virtual UserProfile UserProfile { get; set; }
    }
}