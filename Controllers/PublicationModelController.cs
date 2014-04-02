using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FollowPeers.Models;
using System.Web.Security;
using System.Net.Mail;
using System.Net;
using System.Data.Objects;
using System.Data.SqlClient;
using System.Web.Helpers;
using System.IO;

namespace FollowPeers.Controllers
{
    public class PublicationModelController : Controller
    {
        private FollowPeersDBEntities followPeersDB = new FollowPeersDBEntities();
        private static int Sidebarnumber = 1;
        static string Pubfilename = "";
        static string Imagename = "";
        string name = Membership.GetUser().UserName;
        static int pubcategory;
        //
        // GET: /PublicationModel/

        public ActionResult FavouritePub(int id, string Pubname)
        {
            UserProfile user = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            if ((Pubname != "" && Pubname != null))
            {
                Favourite Item = new Favourite();
                Item.ItemLink = "PublicationModel/Details/" + id;
                Item.ItemName = Pubname;
                Item.ItemType = 6;
                Item.ItemTypeId = Convert.ToInt32(id);

                //Add only if Favourite doesn't already exist
                Favourite FoundMatch = user.Favourites.FirstOrDefault(p => p.ItemTypeId == Item.ItemTypeId && p.ItemType == Item.ItemType);
                if (FoundMatch != null)
                {
                    CreateUpdates("The publication " + Pubname + " is already a favourite", "/PublicationModel/Details/" + id, 6, user.UserProfileId, null);
                    return RedirectToAction("Index", "Profile", new { id = user.UserProfileId });
                }
                user.Favourites.Add(Item);
                followPeersDB.Entry(user).State = EntityState.Modified;
                followPeersDB.SaveChanges();

                ViewBag.FavouriteAdded = "true";
                CreateUpdates("Favourited a new publication titled " + Pubname, "/PublicationModel/Details/" + id, 6, user.UserProfileId, null);
            }
            return RedirectToAction("Details", "PublicationModel", new { id = id });
        }

        public ActionResult UnFavouritePub(int id)
        {
            List<FollowPeers.Models.Favourite> FavouriteList = new List<FollowPeers.Models.Favourite>();
            FavouriteList = followPeersDB.Favourites.Where(p => p.ItemTypeId == id).ToList();

            foreach (Favourite favPub in FavouriteList)
            {
                followPeersDB.Favourites.Remove(favPub);
            }
            followPeersDB.SaveChanges();
            return RedirectToAction("Details", "PublicationModel", new { id = id });
        }

        public ActionResult Recommend(int id, string Names)
        {
            UserProfile user = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            string[] usersToRec = Names.Split(';');
            if (usersToRec.Length != 0)
            {
                foreach (string fullUserName in usersToRec)
                {
                    String[] splitNames = fullUserName.Split(' ');
                    if (splitNames.Length > 0)
                    {
                        String userFirstName = splitNames[0];
                        UserProfile userFromFirstName = followPeersDB.UserProfiles.SingleOrDefault(p => p.FirstName == userFirstName);
                        if (userFromFirstName == null)
                        {
                            return RedirectToAction("Index", "Profile", new { id = user.UserProfileId });
                        }
                        if (splitNames.Length > 1)
                        {
                            String userLastName = splitNames[1];
                            UserProfile userFromLastName = followPeersDB.UserProfiles.SingleOrDefault(p => p.LastName == userLastName);
                            if (userFromFirstName.UserName == userFromLastName.UserName)
                            {
                                return RedirectToAction("Index", "Profile", new { id = user.UserProfileId });
                            }

                        }

                        int recommendid = userFromFirstName.UserProfileId;
                        UserProfile invitee = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserProfileId == recommendid);
                        Bookmark model = new Bookmark();

                        try
                        {
                            if (user.UserProfileId != recommendid)
                            {
                                model.bookmarkType = "Publication";
                                model.itemID = id;
                                model.userID = recommendid;
                                followPeersDB.Bookmarks.Add(model);
                                followPeersDB.SaveChanges();

                                //Adding a notification item to the recommended person
                                PublicationModel book = followPeersDB.PublicationModels.SingleOrDefault(b => b.publicationID == id);
                                Notification newnoti = new Notification
                                {
                                    message = user.FirstName + user.LastName + " has recommeded you a publication : " + book.title,
                                    link = "/PublicationModel/Details/" + id,
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
                }
            }
            return RedirectToAction("Index", "Profile", new { id = user.UserProfileId });
        }


        [HttpPost]
        public JsonResult RecommendedNames(string searchText, int maxResults)
        {
            var result = from n in followPeersDB.UserProfiles
                         where ((n.FirstName.Contains(searchText)) || (n.LastName.Contains(searchText)) || ((n.FirstName + " " + n.LastName).Contains(searchText)))
                         orderby n.FirstName
                         select new
                         {
                             value = n.UserProfileId,
                             name = n.FirstName + " " + n.LastName,

                         };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UploadFile(FormCollection formCollection)
        {
            string name = Membership.GetUser().UserName;
            string email_id = Membership.GetUser().Email;
            UserProfile userprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            HttpFileCollectionBase uploadFile = Request.Files;
            if (uploadFile.Count > 0)
            {
                HttpPostedFileBase postedFile = uploadFile[0];
                System.IO.Stream inStream = postedFile.InputStream;
                byte[] fileData = new byte[postedFile.ContentLength];
                inStream.Read(fileData, 0, postedFile.ContentLength);
                string filename = postedFile.FileName;
                string test = HttpRuntime.AppDomainAppPath;

                string path = test + "Content\\Files\\";
                //System.IO.File.AppendAllText(@"C:\Users\HP User\Documents\GitHub\March Latest\Content\Files\uploadedList.txt", filename + "\r\n");
                //postedFile.SaveAs(@"C:\Users\HP User\Documents\GitHub\March Latest\Content\Files\" + postedFile.FileName);

                var directoryInfo = new DirectoryInfo(path);

                if (directoryInfo.Exists)
                {
                    Console.WriteLine("Create a directory");
                    directoryInfo.CreateSubdirectory(email_id);

                    string path1 = path + email_id;
                    var newdirectoryinfo = new DirectoryInfo(path1);
                    Console.WriteLine("Create a directory");
                    newdirectoryinfo.CreateSubdirectory("Publications");
                    path1 = path1 + "\\Publications\\";
                    Console.WriteLine("New path", path1);

                    // System.IO.File.AppendAllText(path + email_id + "\\uploadedList.txt", filename + "\r\n");
                    postedFile.SaveAs(path1 + postedFile.FileName);
                    Pubfilename = filename;
                }

            }

            //return View("Index", new { id = userprofile.UserProfileId });
            //return RedirectToAction("Index", "Profile");
            if (Imagename != "")
                return RedirectToAction("Create", "PublicationModel", new { id = Sidebarnumber, upload = 1 });
            else
                return RedirectToAction("Create", "PublicationModel", new { id = Sidebarnumber, upload = 4 });
            //return View(userprofile);

        }

        public ActionResult UploadPhoto(FormCollection formCollection)
        {
            string name = Membership.GetUser().UserName;
            string email_id = Membership.GetUser().Email;
            UserProfile userprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            HttpFileCollectionBase uploadFile = Request.Files;
            if (uploadFile.Count > 0)
            {
                HttpPostedFileBase postedFile = uploadFile[0];
                System.IO.Stream inStream = postedFile.InputStream;
                byte[] fileData = new byte[postedFile.ContentLength];
                inStream.Read(fileData, 0, postedFile.ContentLength);
                string filename = postedFile.FileName;
                string test = HttpRuntime.AppDomainAppPath;

                string path = test + "Content\\Files\\";

                var directoryInfo = new DirectoryInfo(path);

                if (directoryInfo.Exists)
                {
                    Console.WriteLine("Create a directory");
                    directoryInfo.CreateSubdirectory(email_id);

                    string path1 = path + email_id;
                    var newdirectoryinfo = new DirectoryInfo(path1);
                    Console.WriteLine("Create a directory");
                    newdirectoryinfo.CreateSubdirectory("Publications");
                    path1 = path1 + "\\Publications\\";
                    Console.WriteLine("New path", path1);

                    // System.IO.File.AppendAllText(path + email_id + "\\uploadedList.txt", filename + "\r\n");
                    postedFile.SaveAs(path1 + postedFile.FileName);
                    Imagename = filename;
                }

            }

            //return View("Index", new { id = userprofile.UserProfileId });
            //return RedirectToAction("Index", "Profile");
            if (Pubfilename != "" && Pubfilename != null)
                return RedirectToAction("Create", "PublicationModel", new { id = Sidebarnumber, upload = 2 });
            else
                return RedirectToAction("Create", "PublicationModel", new { id = Sidebarnumber, upload = 3 });
            //return View(userprofile);

        }



        public ViewResult Index()
        {
            string name = Membership.GetUser().UserName;
            int userID;
            UserProfile user = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            userID = user.UserProfileId;
            var result = from n in followPeersDB.PublicationModels
                         orderby n.viewCount descending
                         select n;

            return View(result.ToList());
            //return View(followPeersDB.PublicationModels.ToList());
        }

        public ActionResult Rank(int id, int pubId)
        {
            PublicationModel publicationmodel = followPeersDB.PublicationModels.Find(pubId);
            publicationmodel.Rank = (publicationmodel.Rank + id) / 2;
            followPeersDB.SaveChanges();
            return Json(pubId);
        }

        //
        // GET: /PublicationModel/Details/5

        public ViewResult Details(int id)
        {
            PublicationModel publicationmodel = followPeersDB.PublicationModels.Find(id);
            publicationmodel.viewCount = publicationmodel.viewCount + 1;

            string name = Membership.GetUser().UserName;
            UserProfile user = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            followPeersDB.Entry(user).State = EntityState.Modified;
            followPeersDB.SaveChanges();

            ViewBag.bookmarktag = false;
            // Check if a bookmark with these credentials exsists in the Db
            Bookmark bookmark = followPeersDB.Bookmarks.SingleOrDefault(b => b.itemID == id && b.userID == user.UserProfileId && b.bookmarkType == "Publication");

            if (bookmark == null)
            {
                ViewBag.bookmarktag = true;
            }

            return View(publicationmodel);
        }

        //
        // GET: /PublicationModel/Create

        public ActionResult Create(int id, int upload = 0)
        {
            if (upload == 0)
            {
                Imagename = "";
                Pubfilename = "";
            }
            string name = Membership.GetUser().UserName;
            UserProfile user = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);

            int userID;
            userID = user.UserProfileId;
            ViewData["Filename"] = "";
            if (upload == 1)
            {
                ViewData["Upload"] = 1;
                if (Pubfilename != "")
                    ViewData["Filename"] = Pubfilename;
            }
            ViewData["Upload"] = upload;
            ViewData["SideBar"] = id;
            Sidebarnumber = id;

            ViewBag.userID = userID;
            return View();
        }


        public FileResult Download(string downloadPath)
        {
            string filename = downloadPath;
            string contentType = "application/octet-stream";
            string ext = Path.GetExtension(filename).ToLower();

            Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (rk != null && rk.GetValue("Content Type") != null)
                contentType = rk.GetValue("Content Type").ToString();

            //Parameters to file are
            //1. The File Path on the File Server
            //2. The connent type MIME type
            //3. The paraneter for the file save asked by the browser
            return File(filename, contentType);
        }

        //
        // POST: /PublicationModel/Create

        [HttpPost]
        public ActionResult Create(PublicationModel publicationmodel, string[] keywordsId, string keyword, FormCollection formCollection)
        {
            if (ModelState.IsValid)
            {
                if (publicationmodel.author == null)
                {
                    publicationmodel.author = "default";
                }
                if (publicationmodel.conference == null)
                {
                    publicationmodel.conference = "default";
                }
                if (publicationmodel.description == null)
                {
                    publicationmodel.description = "default";
                }
                if (publicationmodel.issue == null)
                {
                    publicationmodel.issue = "default";
                }
                if (publicationmodel.journal == null)
                {
                    publicationmodel.journal = "default";
                }
                if (publicationmodel.keyword == null)
                {
                    publicationmodel.keyword = "default";
                }
                if (publicationmodel.page == null)
                {
                    publicationmodel.page = "default";
                }
                if (publicationmodel.publisher == null)
                {
                    publicationmodel.publisher = "default";
                }
                if (publicationmodel.referenceID == null)
                {
                    publicationmodel.referenceID = "Singapore";
                }
                if (publicationmodel.status == null)
                {
                    publicationmodel.status = "default";
                }
                if (publicationmodel.title == null)
                {
                    publicationmodel.title = "default";
                }
                if (publicationmodel.type == null)
                {
                    publicationmodel.type = "default";
                }
                if (publicationmodel.university == null)
                {
                    publicationmodel.university = "default";
                }
                if (publicationmodel.volume == null)
                {
                    publicationmodel.volume = "default";
                }
                if (publicationmodel.year == null)
                {
                    publicationmodel.year = 2014;
                }
                if (publicationmodel.viewCount == 0)
                {
                    publicationmodel.viewCount = 0;
                }
                if (publicationmodel.specialisation == null)
                {
                    publicationmodel.specialisation = "Physics";
                }
                publicationmodel.timestamp = DateTime.Now.ToString();
                if (Pubfilename != "")
                {
                    publicationmodel.filename = Pubfilename;
                    Pubfilename = "";
                }
                if (Imagename != "")
                {
                    publicationmodel.imagename = Imagename;
                    Imagename = "";
                }

                //Adding Tags
                //To keep track of tags being added
                List<Tag> AddedTags = new List<Tag>();

                //Adding the Tag
                if (keywordsId != null)
                {
                    //Selected Existing Tags
                    foreach (string tag in keywordsId)
                    {
                        int TagId = Convert.ToInt32(tag);
                        Tag Item = followPeersDB.Tags.FirstOrDefault(p => p.TagId == TagId);
                        if (Item != null && !(Item.Publications.Any(p => p.publicationID == publicationmodel.publicationID)))
                        {
                            AddedTags.Add(Item);
                            //When Tag Linked to Item, incremented to keep count
                            Item.TagLinkedItems += 1;
                            publicationmodel.Tags.Add(Item);
                            Item.Publications.Add(publicationmodel);
                        }
                    }
                }
                if (keyword != null)
                {
                    //If written new tag
                    string[] Tags = keyword.Split(';');
                    foreach (string tagname in Tags)
                    {
                        string Trimtagname = tagname.Trim();
                        Tag Item = followPeersDB.Tags.FirstOrDefault(p => p.TagName.ToLower() == Trimtagname.ToLower());
                        if (Item != null)
                        {
                            if (AddedTags.Contains(Item) != true && !(Item.Publications.Any(p => p.publicationID == publicationmodel.publicationID)))
                            {
                                Item.TagLinkedItems += 1;
                                publicationmodel.Tags.Add(Item);
                                Item.Publications.Add(publicationmodel);
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
                            publicationmodel.Tags.Add(NewItem);
                            NewItem.Publications.Add(publicationmodel);
                            AddedTags.Add(NewItem);
                        }
                    }
                }
                //-----------End of Adding Tags

                string name = Membership.GetUser().UserName;
                UserProfile user = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
                publicationmodel.UserProfile = user;
                user.Publication.Add(publicationmodel);

                int publicationmodelid = followPeersDB.PublicationModels.Count() + 1;

                CreateUpdates("Published a new publication titled " + publicationmodel.title, "/PublicationModel/Details/" + publicationmodelid, 6, user.UserProfileId, publicationmodel.keyword);

                //followPeersDB.PublicationModels.Add(publicationmodel);
                followPeersDB.Entry(user).State = EntityState.Modified;
                followPeersDB.SaveChanges();

                return RedirectToAction("Details", "PublicationModel", new { id = publicationmodel.publicationID });
                //return RedirectToAction("Index");
            }

            string name2 = Membership.GetUser().UserName;
            UserProfile user2 = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name2);
            return RedirectToAction("Index", "Profile", new { id = user2.UserProfileId });
            //return View(publicationmodel);

        }

        public ActionResult AddTopic(int id)
        {
            ViewBag.pubId = id;
            PublicationModel pub = followPeersDB.PublicationModels.SingleOrDefault(p => p.publicationID == id);
            ViewBag.CategoryList = followPeersDB.Specializations.Select(s => s.SpecializationName).ToList();
            ViewData["Category"] = pub.specialisation;
            pubcategory = id;
            return View();
        }

        [HttpPost]
        public ActionResult AddTopic(ForumTopic forumTopic)
        {
            string name = Membership.GetUser().UserName;
            UserProfile myprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            forumTopic.createdUser = myprofile;
            forumTopic.groupId = pubcategory.ToString();
            ViewBag.CategoryList = followPeersDB.Specializations.Select(s => s.SpecializationName).ToList();
            if (forumTopic.Category != null && forumTopic.Category != "")
            {
                var toAddInto = followPeersDB.Forums.Single(f => f.Category == forumTopic.Category);
                toAddInto.Topics.Add(forumTopic);
                followPeersDB.SaveChanges();
                ViewBag.TopicAdded = "true";
            }

            return RedirectToAction("TopicDetail", "Forum", new { id = forumTopic.ForumTopicId });
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

            CategoryPost post = new CategoryPost
            {
                TimeStamp = DateTime.Now,
                PostMessage = message,
                Link = link,
                Type = type,
                Postedby = myprofile.UserProfileId,
                Category = category,
            };

            userprofile.CategoryPosts.Add(post);

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

        internal List<Country> FindCountry(string searchText, int maxResults)
        {
            var result = from n in followPeersDB.Countries
                         where n.Name.Contains(searchText)
                         orderby n.Name
                         select n;

            return result.Take(maxResults).ToList();

        }
        [HttpPost]
        public JsonResult FindCountryNames(string searchText, int maxResults)
        {
            var result = FindCountry(searchText, maxResults);
            return Json(result);
        }

        internal List<Journal> findJournal(string searchText, int maxResults)
        {
            var result = from n in followPeersDB.Journals
                         where n.Name.Contains(searchText)
                         orderby n.Name
                         select n;
            return result.Take(maxResults).ToList();
        }

        [HttpPost]
        public JsonResult findJournalNames(string searchText, int maxResults)
        {
            var result = findJournal(searchText, maxResults);
            return Json(result);
        }

        internal List<Publisher> findPublisher(string searchText, int maxResults)
        {
            var result = from n in followPeersDB.Publishers
                         where n.Name.Contains(searchText)
                         orderby n.Name
                         select n;
            return result.Take(maxResults).ToList();
        }

        [HttpPost]
        public JsonResult findPublisherNames(string searchText, int maxResults)
        {
            var result = findPublisher(searchText, maxResults);
            return Json(result);
        }

        internal List<Conference> findConference(string searchText, int maxResults)
        {
            var result = from n in followPeersDB.Conferences
                         where n.Name.Contains(searchText)
                         orderby n.Name
                         select n;
            return result.Take(maxResults).ToList();
        }

        [HttpPost]
        public JsonResult findConferenceNames(string searchText, int maxResults)
        {
            var result = findConference(searchText, maxResults);
            return Json(result);
        }

        internal List<University> findUniversity(string searchText, int maxResults)
        {
            var result = from n in followPeersDB.Universities
                         where n.Name.Contains(searchText)
                         orderby n.Name
                         select n;
            return result.Take(maxResults).ToList();
        }

        [HttpPost]
        public JsonResult findUniversityNames(string searchText, int maxResults)
        {
            var result = findUniversity(searchText, maxResults);
            return Json(result);
        }


        //
        // GET: /PublicationModel/Edit/5

        public ActionResult Edit(int id)
        {
            PublicationModel publicationmodel = followPeersDB.PublicationModels.Find(id);
            return View(publicationmodel);
        }

        //
        // POST: /PublicationModel/Edit/5

        [HttpPost]
        public ActionResult Edit(PublicationModel publicationmodel)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            if (ModelState.IsValid)
            {
                followPeersDB.Entry(publicationmodel).State = EntityState.Modified;
                followPeersDB.SaveChanges();
                return RedirectToAction("Details", "PublicationModel", new { id = publicationmodel.publicationID });
            }
            return View(publicationmodel);
        }


        //
        // GET: /PublicationModel/Delete/5

        public ActionResult Delete(int id)
        {
            UserProfile userprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            PublicationModel publicationmodel = followPeersDB.PublicationModels.Find(id);
            foreach (var forums in followPeersDB.Forums)
            {
                //var currentForumTopic = followPeersDB.ForumTopics.SingleOrDefault(t => t. == currentuser);
                List<ForumTopic> topics = forums.Topics.ToList();
                foreach (var topic in topics)
                {
                    if (topic.createdUser != null && topic.createdUser == userprofile && topic.groupId == id.ToString())
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
            return RedirectToAction("Index", "Profile", new { id = userprofile.UserProfileId });
        }

        public ActionResult Like(int id, string NameId)
        {
            PublicationModel publicationmodel = followPeersDB.PublicationModels.Find(id);
            publicationmodel.Likes = publicationmodel.Likes + 1;
            string name = Membership.GetUser().UserName;
            int userID;
            UserProfile user = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            userID = user.UserProfileId;
            AchievementLike achievement = new AchievementLike();
            achievement.Type = 6;
            achievement.UserProfileId = userID;
            achievement.AchievementId = id;
            achievement.UserProfile = user;
            user.AchievementLikes.Add(achievement);
            followPeersDB.SaveChanges();
            return Json(id);
        }

        public ActionResult Unlike(int id, int pubId, string NameId)
        {
            PublicationModel publicationmodel = followPeersDB.PublicationModels.Find(pubId);
            publicationmodel.Likes = publicationmodel.Likes - 1;
            AchievementLike achievementmodel = followPeersDB.AchievementLikes.Find(id);
            followPeersDB.AchievementLikes.Remove(achievementmodel);
            followPeersDB.SaveChanges();
            return Json(id);
        }
        //
        // POST: /PublicationModel/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            PublicationModel publicationmodel = followPeersDB.PublicationModels.Find(id);
            String Pubname = publicationmodel.title;
            followPeersDB.PublicationModels.Remove(publicationmodel);

            List<FollowPeers.Models.Favourite> FavouriteList = new List<FollowPeers.Models.Favourite>();
            FavouriteList = followPeersDB.Favourites.Where(p => p.ItemTypeId == id).ToList();

            foreach (Favourite favPub in FavouriteList)
            {
                followPeersDB.Favourites.Remove(favPub);
            }

            followPeersDB.SaveChanges();
            UserProfile user = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            CreateUpdates("Deleted the publication titled " + Pubname, null, 6, user.UserProfileId, null);
            return RedirectToAction("Index", "Profile", new { id = user.UserProfileId });
        }

        protected override void Dispose(bool disposing)
        {
            followPeersDB.Dispose();
            base.Dispose(disposing);
        }

        [HttpGet]
        public ActionResult Search(string searchstring)
        {
            //Added by Ritesh, bug fix on search
            ViewData["keywords"] = searchstring;
            //

            string name = Membership.GetUser().UserName;
            UserProfile userprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            if (searchstring.Count() != 0)
            {
                IEnumerable<PublicationModel> result = from PublicationModel in followPeersDB.PublicationModels
                                                       where (PublicationModel.title.Contains(searchstring) ||
                                                       PublicationModel.description.Contains(searchstring) ||
                                                       PublicationModel.keyword.Contains(searchstring)
                                                       )
                                                       orderby PublicationModel.title
                                                       select PublicationModel;
                ViewBag.searchstring = "Search Results for " + searchstring;
                return View(result);
            }
            else
            {
                IEnumerable<PublicationModel> result = from PublicationModel in followPeersDB.PublicationModels
                                                       orderby PublicationModel.title
                                                       select PublicationModel;
                ViewBag.searchstring = "Showing All Publication";
                return View(result);
            }
        }


        [HttpPost]
        public ActionResult AddBookmark(string ID)
        {
            int id = Convert.ToInt16(ID);
            // Get the user name of the current logged in user
            string name = Membership.GetUser().UserName;
            // Get the entire userprofile associated with this user
            UserProfile user = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);

            // Check if a bookmark with these credentials exsists in the Db
            Bookmark bookmark = followPeersDB.Bookmarks.SingleOrDefault(b => b.itemID == id && b.userID == user.UserProfileId && b.bookmarkType == "Publication");

            if (bookmark == null)
            {
                // No Bookmark exsists,  thereforre add this bookmark into the Db
                Bookmark tempBookmark = new Bookmark();
                tempBookmark.itemID = id;
                tempBookmark.bookmarkType = "Publication";
                tempBookmark.userID = user.UserProfileId;
                followPeersDB.Bookmarks.Add(tempBookmark);
                followPeersDB.SaveChanges();
                //Adding a notification item to the owner of the publication
                PublicationModel book = followPeersDB.PublicationModels.SingleOrDefault(b => b.publicationID == tempBookmark.itemID);
                UserProfile updateowner = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserProfileId == book.ownerID);
                string bookdescription = book.title;
                Notification newnoti = new Notification
                {
                    message = user.FirstName + " bookmarked your publication : " + bookdescription,
                    link = "/Profile/Index/" + user.UserProfileId,
                    New = true,
                    imagelink = user.PhotoUrl,
                };

                updateowner.Notifications.Add(newnoti);
                followPeersDB.Entry(updateowner).State = EntityState.Modified;
                followPeersDB.SaveChanges();
            }


            return RedirectToAction("Index", "PublicationModel");
        }


        [HttpPost]
        public ActionResult DeleteBookmark(string ID)
        {
            int id = Convert.ToInt16(ID);
            // Get the user name of the current logged in user
            string name = Membership.GetUser().UserName;
            // Get the entire userprofile associated with this user
            UserProfile user = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);

            // Check if a bookmark with these credentials exsists in the Db
            Bookmark bookmark = followPeersDB.Bookmarks.SingleOrDefault(b => b.itemID == id && b.userID == user.UserProfileId && b.bookmarkType == "Publication");
            followPeersDB.Bookmarks.Remove(bookmark);
            followPeersDB.SaveChanges();
            return RedirectToAction("Index", "PublicationModel");
        }

    }
}