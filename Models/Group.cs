using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

namespace FollowPeers.Models
{
    public class Group
    {
        [Key]
        public int GroupId { set; get; }
        [Required]
        public string Name { set; get; }

        public int OwnerId { set; get; } //UserProfileId of owner
        public virtual ICollection<UserProfile> Members { get; set; } //UserProfiles of Members
        public string GroupDesc { get; set; }
        public ForumTopic TopicDetail { get; set; }
        public virtual ICollection<Tag> Tags { get; set; }

        public Group()
        {
            Tags = new List<Tag>();
        }
    }
}