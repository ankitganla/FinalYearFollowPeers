using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;



namespace FollowPeers.Models
{
    public class Education
    {
        
        public int EducationId { set; get; }

        [Required]
        public int UserProfileId { set; get; }

        [Required]
        public string UniversityName { set; get; }


        public int startYear { set; get; }
        public int endYear { set; get; }
        public string Degree { set; get; }
        public string country { set; get; }
        public virtual UserProfile UserProfile { get; set; }
    }
}