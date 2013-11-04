using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace FollowPeers.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }
        public string EventName { get; set; }
        public string EventLocation {get; set; }
        public DateTime EventDate { get; set; }
        public DateTime EventTime { get; set; }
        public string EventDesc { get; set; }
        public virtual ICollection<UserProfile> Invitees { get; set; }
        public virtual ICollection<Tag> Tags { get; set; }

        public Event()
        {
            Invitees = new List<UserProfile>();
            Tags = new List<Tag>();
        }
    }
}