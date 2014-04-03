using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace FollowPeers.Models
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; }
        [Required]
        public string CourseName { get; set; }
        [Required]
        public string CourseDescription { get; set; }
        //public int attendingCount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public virtual UserProfile Owner { get; set; }
        public virtual ICollection<Tag> Tags { get; set; }

        public Course()
        {
            Tags = new List<Tag>();
        }

    }
}