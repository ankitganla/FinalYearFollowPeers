using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FollowPeers.Models;
using System.Web.Security;

namespace FollowPeers.Controllers
{
    public class GroupsController : Controller
    {
        private FollowPeersDBEntities followPeersDB = new FollowPeersDBEntities();
        //FollowPeersDBEntities db = new FollowPeersDBEntities();
        static UserProfile myprofile;
        static string keyword = null;
        static string Jobtitle = null;
        static string Country = null;
        static string Specialisation = null;
        //
        // GET: /Default1/

        public ActionResult AppliedGroup(int id)
        {
            string name = Membership.GetUser().UserName;
            myprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            // UserProfile followerProfile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == username);
            // UserProfile followerProfile = new UserProfile();
            Group group = followPeersDB.Groups.SingleOrDefault(p => p.GroupId == id);
            bool x = false;
            if (group.Members.Contains(myprofile) && myprofile.Groups.Contains(group))
            {
                group.Members.Remove(myprofile);
                myprofile.Groups.Remove(group);
                followPeersDB.SaveChanges();

                x = true;
            }
            else
            {
                group.Members.Add(myprofile);
                myprofile.Groups.Add(group);
                followPeersDB.SaveChanges();
            }


            UserProfile updateowner = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserProfileId == group.OwnerId);
            string grouptitle = group.Name;
            string makeMessage = "";
            if (x == false)
                makeMessage = myprofile.FirstName + " Joined group : " + grouptitle;
            else
                makeMessage = myprofile.FirstName + " UnJoined group : " + grouptitle;

            Notification newnoti = new Notification
            {
                message = makeMessage,
                link = "/Profile/Index/" + myprofile.UserProfileId,
                New = true,
                imagelink = myprofile.PhotoUrl,
            };

            updateowner.Notifications.Add(newnoti);
            followPeersDB.Entry(updateowner).State = EntityState.Modified;
            followPeersDB.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult AddTopic(string GroupId)
        {


            ViewBag.CategoryList = followPeersDB.Specializations.Select(s => s.SpecializationName).ToList();
            ViewBag.GroupID = GroupId;
            return View();
        }

        [HttpPost]
        public ActionResult AddTopic(ForumTopic forumTopic)
        {
            //string[] splits = values.Split(':');
            //ForumTopic forumTopic = new ForumTopic();
            //if (splits.Length == 3)
            //{

            //    forumTopic.Category = splits[0];
            //    forumTopic.Name = splits[1];
            //    forumTopic.Description = splits[2];
            //}
            string name = Membership.GetUser().UserName;
            myprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            forumTopic.createdUser = myprofile;
            //forumTopic.groupId = ViewBag.GroupID;
            ViewBag.CategoryList = followPeersDB.Specializations.Select(s => s.SpecializationName).ToList();

            if (ModelState.IsValid && forumTopic.Category != null && forumTopic.Category != "")
            {
                var toAddInto = followPeersDB.Forums.Single(f => f.Category == forumTopic.Category);
                toAddInto.Topics.Add(forumTopic);
                followPeersDB.SaveChanges();
                ViewBag.TopicAdded = "true";
            }

            Console.WriteLine(forumTopic.createdUser.FirstName);
            return RedirectToAction("TopicDetail", "Forum", new { id = forumTopic.ForumTopicId });

            //return View(forumTopic);
        }


        public ActionResult SearchTemp(string id)
        {
            keyword = id;
            return Json(keyword);
        }

        public ActionResult AdvanceTemp(string id, string jobtitle, string country)
        {
            keyword = id;
            Country = country;
            Jobtitle = jobtitle;
            return Json(keyword);
        }

        public ViewResult SearchGroups()
        {
            ViewData["keywords"] = keyword;
            //

            string name = Membership.GetUser().UserName;
            UserProfile userprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            if (keyword.Count() != 0)
            {
                var result = from Group in followPeersDB.Groups
                             where (Group.Name.Contains(keyword) ||
                             Group.GroupDesc.Contains(keyword)
                             )
                             orderby Group.Name
                             select Group;
                ViewBag.searchstring = "Search Results for " + keyword;
                return View(result.ToList());
            }
            else
            {
                var result = from Group in followPeersDB.Groups
                             orderby Group.Name
                             select Group;
                ViewBag.searchstring = "Showing All Groups";
                return View(result.ToList());
            }
        }
        public ActionResult SavedJob(int id)
        {
            string name = Membership.GetUser().UserName;
            myprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            // UserProfile followerProfile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == username);
            // UserProfile followerProfile = new UserProfile();
            Group group = followPeersDB.Groups.SingleOrDefault(p => p.GroupId == id);
            myprofile.Groups.Add(group);
            followPeersDB.SaveChanges();
            return RedirectToAction("Index");
        }

        public ViewResult Index()
        {
            string name = Membership.GetUser().UserName;
            int userID;
            UserProfile user = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            userID = user.UserProfileId;
            var result = from n in followPeersDB.Groups select n;
            if (result != null)
                return View(result.ToList());
            else
                return View();
        }
        //
        // GET: /Default1/Details/5

        public ActionResult Create()
        {
            string name = Membership.GetUser().UserName;
            UserProfile user = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);

            int userID;
            userID = user.UserProfileId;

            ViewBag.userID = userID;
            return View();
        }


        //
        // POST: /Default1/Create


        [HttpPost]
        public ActionResult Create(Group groupmodel)
        {
            if (ModelState.IsValid)
            {
                string name = Membership.GetUser().UserName;
                UserProfile user = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
                /*if (jobmodel.Enddate == null)
                {
                    jobmodel.Enddate = DateTime.Now;
                }
                if (jobmodel.publishDate == null)
                {
                    jobmodel.publishDate = DateTime.Now;
                }
                 */

                groupmodel.OwnerId = user.UserProfileId;

                int groupid = followPeersDB.Groups.Count() + 1;
                user.Groups.Add(groupmodel);
                //CreateUpdates("Published a new job titled " + groupmodel.Title, "/PublicationModel/Details/" + groupid, 6, user.UserProfileId);
                //followPeersDB.Entry(user).State = EntityState.Modified;
                CreateUpdates(groupmodel, groupmodel.GroupDesc);
                followPeersDB.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(groupmodel);
        }

        public void CreateUpdates(Group model, string category)
        {
            if (category != null)
            {
                string GroupOwner = Membership.GetUser().UserName;
                myprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == GroupOwner);
                CategoryPost post = new CategoryPost
                {
                    TimeStamp = DateTime.Now,
                    PostMessage = "New Group Has Recently Been Created titled" + model.Name,
                    Link = "/Groups/Details/"+model.GroupId,
                    Type = 2,
                    Postedby = myprofile.UserProfileId,
                    Category = category,
                };

                myprofile.CategoryPosts.Add(post);
            }
        }

        /*
        public void CreateUpdates(string message, string link, int type, int id)
        {
            string myname = Membership.GetUser().UserName;
            UserProfile userprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserProfileId == id);
            UserProfile myprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == myname);
            Update record = new Update
            {
                Time = DateTime.Now,
                message = message,
                link = link,
                New = true,
                Own = true,
                type = type,
                owner = myprofile.UserProfileId
            };

            userprofile.Updates.Add(record); //add own update record

            string name = userprofile.UserName;
            IQueryable<string> custQuery = from cust in followPeersDB.Relationships where cust.personBName == name select cust.personAName;

            foreach (string personAname in custQuery)
            {
                Update record2 = new Update
                {
                    Time = DateTime.Now,
                    message = message,
                    link = link,
                    New = true,
                    Own = false,
                    type = type,
                    owner = userprofile.UserProfileId
                };

                UserProfile tempuserprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == personAname);

                tempuserprofile.Updates.Add(record2);
            } //add a new update record for the followers            
        }

         * */
        //
        // GET: /Default1/Edit/5


        //
        // GET: /Default1/Delete/5

        public ViewResult Details(int id)
        {
            Group groupmodel = followPeersDB.Groups.Find(id);

            string name = Membership.GetUser().UserName;
            UserProfile user = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            followPeersDB.Entry(user).State = EntityState.Modified;
            followPeersDB.SaveChanges();

            ViewBag.bookmarktag = false;
            // Check if a bookmark with these credentials exsists in the Db
            //Bookmark bookmark = followPeersDB.Bookmarks.SingleOrDefault(b => b.itemID == id && b.userID == user.UserProfileId);
            /*
            if (bookmark == null)
            {
                ViewBag.bookmarktag = true;
            }
            */

            return View(groupmodel);
        }

        public ActionResult Delete(int id)
        {
            Group groupmodel = followPeersDB.Groups.Find(id);
            followPeersDB.Groups.Remove(groupmodel);
            string name = Membership.GetUser().UserName;
            UserProfile user = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);

            foreach (var forums in followPeersDB.Forums)
            {
                //var currentForumTopic = followPeersDB.ForumTopics.SingleOrDefault(t => t. == currentuser);
                List<ForumTopic> topics = forums.Topics.ToList();
                foreach (var topic in topics)
                {
                    if (topic.createdUser != null && topic.createdUser == user && topic.groupId == id.ToString())
                    {
                        List<ForumPost> posts = followPeersDB.ForumPosts.ToList();
                        foreach (var post in posts)
                        {
                            if (topic.Posts.Contains(post))
                                followPeersDB.ForumPosts.Remove(post);
                        }
                        followPeersDB.ForumTopics.Remove(topic);

                    }
                }
            }

            followPeersDB.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            Group groupmodel = followPeersDB.Groups.Find(id);
            return View(groupmodel);
        }

        [HttpPost]
        public ActionResult Edit(Group groupmodel)
        {
            if (ModelState.IsValid)
            {
                followPeersDB.Entry(groupmodel).State = EntityState.Modified;
                followPeersDB.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(groupmodel);
        }


        //public ActionResult AdvanceSearch()
        //{
        //    string keywords = keyword;
        //    string country = Country;
        //    string specialization = null;
        //    string jobtitle = Jobtitle;
        //    int sort = 0;
        //    ViewBag.sort = sort;
        //    string name = Membership.GetUser().UserName;
        //    UserProfile userprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
        //    string Keywords = keywords;
        //    Specialization spec = null;
        //    IEnumerable<Jobs> result = null;
        //    string searchstring = "Job Search Results ";
        //    if ((keywords != null) && (keywords != "") && (keywords.Length != 0))
        //    {
        //        Keywords = keywords;
        //        ViewData["keywords"] = keywords;
        //        searchstring += "For " + keywords;
        //    }
        //    if ((specialization != null) && (specialization != "Any"))
        //    {
        //        IEnumerable<Jobs> tempresult = null;
        //        spec = followPeersDB.Specializations.First(p => p.SpecializationName.Contains(specialization));
        //        tempresult = from j in followPeersDB.Jobs
        //                     where ((j.Title.Contains(Keywords) || (j.Description.Contains(Keywords))))
        //                     orderby j.Title
        //                     select j;
        //        searchstring += " " + specialization;
        //        if (country != null)
        //        {
        //            tempresult = from j in followPeersDB.Jobs
        //                         where ((j.Title.Contains(Keywords) || (j.Description.Contains(Keywords))) && (j.Country.Contains(country)))
        //                         orderby j.Title
        //                         select j;
        //            searchstring = searchstring + " in " + country;
        //        }
        //        if (jobtitle != null)
        //        {
        //            tempresult = from j in followPeersDB.Jobs
        //                         where ((j.Title.Contains(jobtitle) || (j.Description.Contains(Keywords))) && (j.Country.Contains(country)))
        //                         orderby j.Title
        //                         select j;
        //        }

        //        result = null;
        //        List<Jobs> temp = new List<Jobs>();
        //        foreach (var r in tempresult)
        //        {
        //            /*if (r.Specializations.Contains(spec))
        //            {
        //                temp.Add(r);
        //            }*/

        //        }
        //        foreach (var u in temp)
        //        {
        //            string temp1 = u.JobId.ToString();
        //            if (userprofile.Jobs.Contains(u) && u.ownerID != userprofile.UserProfileId)//need to add a button
        //                ViewData[temp1] = "1";
        //            else
        //                ViewData[temp1] = "0";
        //        }
        //        if (searchstring == "Job Search Results ") searchstring = "Showing all jobs in the database";
        //        ViewBag.searchstring = searchstring;
        //        return View("SearchJob", temp);
        //    }

        //    else
        //    {
        //        result = from j in followPeersDB.Jobs
        //                 where ((j.Title.Contains(Keywords) || (j.Description.Contains(Keywords))))
        //                 orderby j.Title
        //                 select j;
        //        if (country != null)
        //        {
        //            result = from j in followPeersDB.Jobs
        //                     where (j.Country.Contains(country) && (j.Title.Contains(Keywords) || (j.Description.Contains(Keywords))))
        //                     orderby j.Title
        //                     select j;
        //            searchstring = searchstring + " in " + country;
        //        }
        //    }
        //    foreach (var u in result)
        //    {
        //        string temp = u.JobId.ToString();
        //        if (userprofile.Jobs.Contains(u) && u.ownerID != userprofile.UserProfileId)//need to add a button
        //            ViewData[temp] = "1";
        //        else
        //            ViewData[temp] = "0";
        //    }
        //    if (searchstring == "Job Search Results ") searchstring = "Showing all jobs in the database";
        //    ViewBag.searchstring = searchstring;
        //    return View("SearchJob", result);
        //}

    }
}
