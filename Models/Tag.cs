using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace FollowPeers.Models
{
    public class Tag
    {
        [Key]
        public int TagId { get; set; }
        public string TagName { get; set; }
        public int TagLinkedItems { get; set; } //Every time a tag is linked to an item, it is incremented. Used for Popular Tags
        public virtual ICollection<PublicationModel> Publications { get; set; }
        public virtual ICollection<PatentModel> Patents { get; set; }
        public virtual ICollection<Course> Courses { get; set; }
        public virtual ICollection<Jobs> Jobs { get; set; }
        public virtual ICollection<Group> Groups { get; set; }
        public virtual ICollection<UserProfile> Users { get; set; }
        public virtual ICollection<Event> Events { get; set; }

        public Tag()
        {
            Publications = new List<PublicationModel>();
            Jobs = new List<Jobs>();
            Events = new List<Event>();
            Patents = new List<PatentModel>();
            Courses = new List<Course>();
            Groups = new List<Group>();
            Users = new List<UserProfile>();
        }
    }
}