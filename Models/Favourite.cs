using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace FollowPeers.Models
{
    public class Favourite
    {
        [Key]
        public int ItemId { get; set; }
        public int ItemType { get; set; }
        public string ItemName { get; set; }
        public string ItemImageLink { get; set; } //for thumbnail image
        public string ItemLink { get; set; }
        public string ItemResearchInterests { get; set; }
        public int ItemTypeId { get; set; }
        public virtual UserProfile UserProfile { get; set; }
    }
}
//item type
//publication: 6
//savedjob: 4
//course: 13