using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace FollowPeers.Models
{
    public class EventUserProfileJoin
    {
        [Key, Column(Order = 0)]
        public int UserProfileId { set; get; }
        [Key, Column(Order = 1)]
        public int EventId { set; get; }
        public int JoinStatus { get; set; }
    }
}