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
    public class JobsController : Controller
    {
        private FollowPeersDBEntities followPeersDB = new FollowPeersDBEntities();
        private static int Sidebarnumber = 1;
        string name = Membership.GetUser().UserName;
        static UserProfile myprofile;
        static string keyword = null;
        static string Jobtitle = null;
        static string Country = null;
        static string Specialisation = null;
        //
        // GET: /Default1/

        public ActionResult AppliedJob(int id)
        {

            myprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            // UserProfile followerProfile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == username);
            // UserProfile followerProfile = new UserProfile();
            Jobs job = followPeersDB.Jobs.SingleOrDefault(p => p.JobId == id);
            myprofile.AppliedJobs.Add(job);
            followPeersDB.SaveChanges();

            UserProfile updateowner = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserProfileId == job.ownerID);
            string jobtitle = job.Title;
            Notification newnoti = new Notification
            {
                message = myprofile.FirstName + " Applied to your job post : " + jobtitle,
                link = "/Profile/Index/" + myprofile.UserProfileId,
                New = true,
                imagelink = myprofile.PhotoUrl,
            };

            updateowner.Notifications.Add(newnoti);
            followPeersDB.Entry(updateowner).State = EntityState.Modified;
            followPeersDB.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Recommend(int id, string NameId)
        {
            UserProfile user = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            Bookmark model = new Bookmark();

            if (NameId != null && NameId != "")
            {
                int recommendid = Convert.ToInt32(NameId);
                UserProfile invitee = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserProfileId == recommendid);

                try
                {
                    if (user.UserProfileId != recommendid)
                    {
                        model.bookmarkType = "Job";
                        model.itemID = id;
                        model.userID = recommendid;
                        followPeersDB.Bookmarks.Add(model);
                        followPeersDB.SaveChanges();

                        //Adding a notification item to the recommended person
                        Jobs job = followPeersDB.Jobs.SingleOrDefault(b => b.JobId == id);
                        Notification newnoti = new Notification
                        {
                            message = user.FirstName + user.LastName + " has recommeded you a job : " + job.Title,
                            link = "/Jobs/Details/" + id,
                            New = true,
                            imagelink = user.PhotoUrl,
                        };

                        invitee.Notifications.Add(newnoti);
                        followPeersDB.Entry(invitee).State = EntityState.Modified;
                        followPeersDB.SaveChanges();
                    }

                }
                catch
                {
                }
            }
            string result = "#";
            return Json(result);
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

        public ViewResult SearchJob()
        {
            ViewData["keywords"] = keyword;
            //

            string name = Membership.GetUser().UserName;
            UserProfile userprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            if (keyword.Count() != 0)
            {
                var result = from Jobs in followPeersDB.Jobs
                             where (Jobs.Title.Contains(keyword) ||
                             Jobs.Description.Contains(keyword) ||
                             Jobs.Company.Contains(keyword)
                             )
                             orderby Jobs.Title
                             select Jobs;
                ViewBag.searchstring = "Search Results for " + keyword;
                return View(result.ToList());
            }
            else
            {
                var result = from Jobs in followPeersDB.Jobs
                             orderby Jobs.Title
                             select Jobs;
                ViewBag.searchstring = "Showing All Publications";
                return View(result.ToList());
            }
        }
        public ActionResult SaveJob(int id, string Jobname)
        {
            string name = Membership.GetUser().UserName;
            UserProfile user = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            if ((Jobname != "" && Jobname != null))
            {
                Favourite Item = new Favourite();
                Item.ItemLink = "Jobs/Details/" + id;
                Item.ItemName = Jobname;
                Item.ItemType = 4;
                Item.ItemTypeId = Convert.ToInt32(id);

                //Add only if Favourite doesn't already exist
                Favourite FoundMatch = user.Favourites.FirstOrDefault(p => p.ItemTypeId == Item.ItemTypeId && p.ItemType == Item.ItemType);
                if (FoundMatch != null)
                {
                    //there is already a fav here
                    return RedirectToAction("Details", "Jobs", new { id = id });
                }

                user.Favourites.Add(Item);
                followPeersDB.Entry(user).State = EntityState.Modified;
                followPeersDB.SaveChanges();
                ViewBag.FavouriteAdded = "true";
            }
            return RedirectToAction("Details", "Jobs", new { id = id });
        }

        public ActionResult UnSaveJob(int id)
        {
            Favourite toUnsave = followPeersDB.Favourites.SingleOrDefault(p => p.ItemTypeId == id && p.ItemType == 4);

            if (toUnsave != null)
            {
                followPeersDB.Favourites.Remove(toUnsave);
                followPeersDB.SaveChanges();
            }

            return RedirectToAction("Details", "Jobs", new { id = id });
        }


        public ViewResult Index()
        {
            string name = Membership.GetUser().UserName;
            int userID;
            UserProfile user = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            userID = user.UserProfileId;
            var result = from n in followPeersDB.Jobs select n;
            if (result != null)
                return View(result.ToList());
            else
                return View();
        }
        //
        // GET: /Default1/Details/5

        public ActionResult Create(int id)
        {
            string name = Membership.GetUser().UserName;
            UserProfile user = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);

            int userID;
            userID = user.UserProfileId;
            ViewData["SideBar"] = id;
            Sidebarnumber = id;
            ViewBag.userID = userID;
            return View();
        }


        //
        // POST: /Default1/Create


        [HttpPost]
        public ActionResult Create(Jobs jobmodel, string[] TagsId, string tag, int jobID)
        {
            jobmodel.JobId = jobID;

            if (ModelState.IsValid)
            {
                string name = Membership.GetUser().UserName;
                UserProfile user = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
                if (jobmodel.Enddate == null)
                {
                    jobmodel.Enddate = DateTime.Now;
                }
                if (jobmodel.publishDate == null)
                {
                    jobmodel.publishDate = DateTime.Now;
                }
                jobmodel.ownerID = user.UserProfileId;

                int jobid = followPeersDB.Jobs.Count() + 1;

                //Adding the Tag
                //To keep track of tags being added
                List<Tag> AddedTags = new List<Tag>();

                if (jobmodel.Description != null && jobmodel.Description != "")
                {
                    string keyword = jobmodel.Description;
                    //If written new tag
                    string[] Tags = keyword.Split(';');
                    foreach (string tagname in Tags)
                    {
                        string Trimtagname = tagname.Trim();
                        Tag Item = followPeersDB.Tags.FirstOrDefault(p => p.TagName.ToLower() == Trimtagname.ToLower());
                        if (Item != null)
                        {
                            if (AddedTags.Contains(Item) != true && !(Item.Jobs.Any(p => p.JobId == jobmodel.JobId)))
                            {
                                Item.TagLinkedItems += 1;
                                jobmodel.Tags.Add(Item);
                                Item.Jobs.Add(jobmodel);
                                AddedTags.Add(Item);
                            }
                        }
                        else //If (Item == null)
                        {
                            //Create a New Tag
                            Tag NewItem = new Tag();
                            NewItem.TagName = Trimtagname;
                            NewItem.TagLinkedItems = 1;
                            followPeersDB.Tags.Add(NewItem);
                            jobmodel.Tags.Add(NewItem);
                            NewItem.Jobs.Add(jobmodel);
                            AddedTags.Add(NewItem);
                        }
                    }
                }
                //-----------End of Adding Tags

                user.Jobs.Add(jobmodel);
                CreateUpdates("Published a new job titled " + jobmodel.Title, "/Jobs/Details/" + jobid, 6, user.UserProfileId, jobmodel.Description);
                //followPeersDB.Entry(user).State = EntityState.Modified;
                followPeersDB.SaveChanges();

                return RedirectToAction("Details", "Jobs", new { id= jobmodel.JobId});
            }
            return View(jobmodel);
        }

        public void CreateUpdates(string message, string link, int type, int id, string category)
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

            //---------------Add To Category Post
            CategoryPost post = new CategoryPost
            {
                TimeStamp = DateTime.Now,
                PostMessage = message,
                Link = link,
                Type = 7,
                Postedby = myprofile.UserProfileId,
                Category = category,
            };

            userprofile.CategoryPosts.Add(post);
            //---------------End OF Category Post

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

        //
        // GET: /Default1/Edit/5


        //
        // GET: /Default1/Delete/5

        public ViewResult Details(int id)
        {
            Jobs jobmodel = followPeersDB.Jobs.Find(id);

            string name = Membership.GetUser().UserName;
            UserProfile user = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            followPeersDB.Entry(user).State = EntityState.Modified;
            followPeersDB.SaveChanges();

            ViewBag.bookmarktag = false;
            // Check if a bookmark with these credentials exsists in the Db
            Bookmark bookmark = followPeersDB.Bookmarks.SingleOrDefault(b => b.itemID == id && b.userID == user.UserProfileId && b.bookmarkType == "Jobs");

            if (bookmark == null)
            {
                ViewBag.bookmarktag = true;
            }

            return View(jobmodel);
        }

        public ActionResult Delete(int id)
        {
            Jobs jobmodel = followPeersDB.Jobs.Find(id);
            int ownersId = jobmodel.ownerID;

            if (jobmodel != null)
            {
                followPeersDB.Jobs.Remove(jobmodel);
                Favourite favmodel = followPeersDB.Favourites.SingleOrDefault(p => p.ItemTypeId == id && p.ItemType == 4);
                if (favmodel != null)
                {
                    followPeersDB.Favourites.Remove(favmodel);
                }
                followPeersDB.SaveChanges();
                return RedirectToAction("Index", "Profile", new { id = ownersId});
            }
            
            return View(jobmodel);
        }

        public ActionResult Edit(int id)
        {
            Jobs jobmodel = followPeersDB.Jobs.Find(id);
            return View(jobmodel);
        }

        [HttpPost]
        public ActionResult Edit(Jobs jobmodel)
        {
            if (ModelState.IsValid)
            {
                followPeersDB.Entry(jobmodel).State = EntityState.Modified;
                followPeersDB.SaveChanges();
                return RedirectToAction("Details", "Jobs", new { id = jobmodel.JobId });
            }
            return View(jobmodel);
        }

        public ActionResult AdvanceSearch()
        {
            string keywords = keyword;
            string country = Country;
            string specialization = null;
            string jobtitle = Jobtitle;
            int sort = 0;
            ViewBag.sort = sort;
            string name = Membership.GetUser().UserName;
            UserProfile userprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            string Keywords = keywords;
            Specialization spec = null;
            IEnumerable<Jobs> result = null;
            string searchstring = "Job Search Results ";
            if ((keywords != null) && (keywords != "") && (keywords.Length != 0))
            {
                Keywords = keywords;
                ViewData["keywords"] = keywords;
                searchstring += "For " + keywords;
            }
            if ((specialization != null) && (specialization != "Any"))
            {
                IEnumerable<Jobs> tempresult = null;
                spec = followPeersDB.Specializations.First(p => p.SpecializationName.Contains(specialization));
                tempresult = from j in followPeersDB.Jobs
                             where ((j.Title.Contains(Keywords) || (j.Description.Contains(Keywords))))
                             orderby j.Title
                             select j;
                searchstring += " " + specialization;
                if (country != null)
                {
                    tempresult = from j in followPeersDB.Jobs
                                 where ((j.Title.Contains(Keywords) || (j.Description.Contains(Keywords))) && (j.Country.Contains(country)))
                                 orderby j.Title
                                 select j;
                    searchstring = searchstring + " in " + country;
                }
                if (jobtitle != null)
                {
                    tempresult = from j in followPeersDB.Jobs
                                 where ((j.Title.Contains(jobtitle) || (j.Description.Contains(Keywords))) && (j.Country.Contains(country)))
                                 orderby j.Title
                                 select j;
                }

                result = null;
                List<Jobs> temp = new List<Jobs>();
                foreach (var r in tempresult)
                {
                    /*if (r.Specializations.Contains(spec))
                    {
                        temp.Add(r);
                    }*/

                }
                foreach (var u in temp)
                {
                    string temp1 = u.JobId.ToString();
                    if (userprofile.Jobs.Contains(u) && u.ownerID != userprofile.UserProfileId)//need to add a button
                        ViewData[temp1] = "1";
                    else
                        ViewData[temp1] = "0";
                }
                if (searchstring == "Job Search Results ") searchstring = "Showing all jobs in the database";
                ViewBag.searchstring = searchstring;
                return View("SearchJob", temp);
            }

            else
            {
                result = from j in followPeersDB.Jobs
                         where ((j.Title.Contains(Keywords) || (j.Description.Contains(Keywords))))
                         orderby j.Title
                         select j;
                if (country != null)
                {
                    result = from j in followPeersDB.Jobs
                             where (j.Country.Contains(country) && (j.Title.Contains(Keywords) || (j.Description.Contains(Keywords))))
                             orderby j.Title
                             select j;
                    searchstring = searchstring + " in " + country;
                }
            }
            foreach (var u in result)
            {
                string temp = u.JobId.ToString();
                if (userprofile.Jobs.Contains(u) && u.ownerID != userprofile.UserProfileId)//need to add a button
                    ViewData[temp] = "1";
                else
                    ViewData[temp] = "0";
            }
            if (searchstring == "Job Search Results ") searchstring = "Showing all jobs in the database";
            ViewBag.searchstring = searchstring;
            return View("SearchJob", result);
        }
    }
}
