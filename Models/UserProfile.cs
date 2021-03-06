﻿using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Web;
using System;

namespace FollowPeers.Models
{

    public class UserProfile
    {
        //Basic info: userId, username, name, languages, thumbnail, profilepic, age, gender, description, location

        [ScaffoldColumn(false)] //means this value will never be displayed in the web browser
        [Key]
        public int UserProfileId { get; set; }


        [ScaffoldColumn(false)]
        [Required]
        public string UserName { get; set; }

        [Required]
        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [Required]
        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [Required]
        public DateTime? Birthday { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public string Status { get; set; }

        [Required]
        public string PhotoUrl { get; set; }

        [Required]
        public string Profession { get; set; }
        
       

        public string StatusMessage { get; set; }
        public int Default { get; set; }
        [ScaffoldColumn(false)]
        public float SignUpProgress { get; set; }
        [ScaffoldColumn(false)]
        public bool activated { get; set; }
        public bool firsttime { get; set; }
        public string Organization { get; set; }
        public string Departments { get; set; }
        public string Country { get; set; }
        [AllowHtml]
        public string AboutMe { get; set; }
        public virtual Contact Contact { get; set; }
        public virtual IList<Education> Educations { get; set; }
        public virtual ICollection<PublicationModel> Publication { get; set; }
        public virtual ICollection<Course> Courses { get; set; }
        public virtual ICollection<PatentModel> Patent { get; set; }
        public virtual ICollection<Update> Updates { get; set; }
        public virtual ICollection<Tier> Tiers { get; set; }
        public virtual ICollection<Specialization> Specializations { get; set; }
        public virtual ICollection<Student> Students { get; set; }
        public virtual ICollection<Group> Groups { get; set; }
        public virtual ICollection<Keyword> Keywords { get; set; }
        public virtual ICollection<Conversation> Conversations { get; set; }
        public virtual ICollection<Portfolio> Portfolios { get; set; }
        public virtual ICollection<Jobs> Jobs { get; set; }
        public virtual ICollection<Jobs> AppliedJobs { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public List<int> PublicationBookmark { get; set; }
        public ICollection<Event> Events { get; set; }
        public virtual ICollection<CategoryPost> CategoryPosts { get; set; }
        public virtual ICollection<Favourite> Favourites { get; set; }
        public virtual ICollection<Achievement> Achievements { get; set; }
        public virtual ICollection<AchievementLike> AchievementLikes { get; set; }
        public virtual ICollection<Employment> Employments { get; set; }

        public UserProfile()
        {
            Events = new List<Event>();
            CategoryPosts = new List<CategoryPost>();
            PublicationBookmark = new List<int>();
            Favourites = new List<Favourite>();
        }

        //public virtual ICollection<Event> Events { get; set; }
    }
    public class Keyword
    {
        public int KeywordId { get; set; }
        public string Area { get; set; }
        public virtual UserProfile UserProfile { get; set; }
    }
}