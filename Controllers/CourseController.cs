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
    public class CourseController : Controller
    {
        private FollowPeersDBEntities db = new FollowPeersDBEntities();
        string name = Membership.GetUser().UserName;
        static UserProfile myprofile;
        //
        // GET: /Course/

        public ViewResult Index()
        {
            myprofile = db.UserProfiles.SingleOrDefault(p => p.UserName == name);
            return View(db.Courses.ToList());
        }

        //
        // GET: /Course/Mine

        public ViewResult Mine()
        {
            myprofile = db.UserProfiles.SingleOrDefault(p => p.UserName == name);
            return View(myprofile.Courses.ToList());
        }


        //
        // GET: /Course/Search

        public ViewResult Search(string q)
        {
            myprofile = db.UserProfiles.SingleOrDefault(p => p.UserName == name);
            if (q == null || q == "")
            {
                ViewBag.SearchString = "Showing all courses";
                ViewBag.CourseCount = db.Courses.ToList().Count;
                return View(db.Courses.ToList());
            }
            else
            {
                ViewBag.SearchString = "Showing results for: " + q;

                IEnumerable<Course> result = from Course in db.Courses
                                             where (Course.CourseName.Contains(q))
                                             orderby Course.CourseName
                                             select Course;

                ViewBag.CourseCount = result.ToList().Count;
                return View(result.ToList());
            }
        }


        //
        // GET: /Course/Details/5

        public ViewResult Details(int id)
        {
            Course course = db.Courses.Find(id);
            return View(course);
        }

        //
        // GET: /Course/Create

        public ActionResult Create()
        {
            string name = Membership.GetUser().UserName;
            UserProfile user = db.UserProfiles.SingleOrDefault(p => p.UserName == name);

            int userID;
            userID = user.UserProfileId;
            ViewBag.UserProfile = user;
            ViewBag.userID = userID;
            return View();

        }

        //
        // POST: /Course/Create

        [HttpPost]
        public ActionResult Create(Course course, FormCollection formCollection)
        {
            string name = Membership.GetUser().UserName;
            UserProfile user = db.UserProfiles.SingleOrDefault(p => p.UserName == name);

            foreach (String key in formCollection.AllKeys)
            {
                switch (key)
                {
                    case "StartDate":
                        try
                        {
                            DateTime dt = DateTime.Parse(formCollection.Get(key));
                            course.StartDate = (DateTime?)dt;
                        }
                        catch
                        {
                        }
                        break;
                    case "EndDate":
                        try
                        {
                            DateTime dt = DateTime.Parse(formCollection.Get(key));
                            course.EndDate = (DateTime?)dt;
                        }
                        catch
                        {
                        }
                        break;
                    default:
                        break;

                }
            }

            if (ModelState.IsValid)
            {
                user.Courses.Add(course);
                db.Entry(user).State = EntityState.Modified;

                //db.Courses.Add(course);
                CreateUpdates(course, course.CourseDescription);

                //Adding the Tag
                //To keep track of tags being added
                List<Tag> AddedTags = new List<Tag>();

                if (course.CourseDescription != null && course.CourseDescription != "")
                {
                    string keyword = course.CourseDescription;
                    //If written new tag
                    string[] Tags = keyword.Split(';');
                    foreach (string tagname in Tags)
                    {
                        string Trimtagname = tagname.Trim();
                        Tag Item = db.Tags.FirstOrDefault(p => p.TagName.ToLower() == Trimtagname.ToLower());
                        if (Item != null)
                        {
                            if (AddedTags.Contains(Item) != true && !(Item.Courses.Any(p => p.CourseId == course.CourseId)))
                            {
                                Item.TagLinkedItems += 1;
                                course.Tags.Add(Item);
                                Item.Courses.Add(course);
                                AddedTags.Add(Item);
                            }
                        }
                        else //If (Item == null)
                        {
                            //Create a New Tag
                            Tag NewItem = new Tag();
                            NewItem.TagName = Trimtagname;
                            NewItem.TagLinkedItems = 1;
                            db.Tags.Add(NewItem);
                            course.Tags.Add(NewItem);
                            NewItem.Courses.Add(course);
                            AddedTags.Add(NewItem);
                        }
                    }
                }
                //-----------End of Adding Tags
                db.SaveChanges();
                return RedirectToAction("Details", "Course", new { id = course.CourseId });
            }

            return View(course);
        }

        public void CreateUpdates(Course model, string category)
        {
            string name = Membership.GetUser().UserName;
            myprofile = db.UserProfiles.SingleOrDefault(p => p.UserName == name);
            string CreateMessage = "New Course Created titled '" + model.CourseName + "'";
            int coursenumber = db.Courses.Count() + 1;
            if (category != null)
            {
                if (category.Contains(';'))
                {
                    string[] Tagnames = category.Split(';');
                    CategoryPost post = new CategoryPost
                    {

                        TimeStamp = DateTime.Now,
                        PostMessage = CreateMessage,
                        Link = "/Course/Details/" + coursenumber,
                        Type = 4,
                        Postedby = myprofile.UserProfileId,
                        Category = Tagnames[0],
                    };

                    myprofile.CategoryPosts.Add(post);
                }
            }


            //For own updates
            Update record = new Update
            {
                Time = DateTime.Now,
                message = CreateMessage,
                link = "/Course/Details/" + coursenumber,
                New = true,
                Own = true,
                type = 6,
                owner = myprofile.UserProfileId
            };

            myprofile.Updates.Add(record); //add own update record

            db.Entry(myprofile).State = EntityState.Modified;
            db.SaveChanges();
        }

        //
        // GET: /Course/CourseAddVideo

        public ActionResult CourseAddVideo(int id)
        {
            Course course = db.Courses.SingleOrDefault(p => p.CourseId == id);
            return View(course);
        }

        //
        // POST: /Course/Create

        [HttpPost]
        public ActionResult CourseAddVideo(HttpPostedFileBase video, int CourseId)
        {
            string name = Membership.GetUser().UserName;
            string email_id = Membership.GetUser().Email;
            UserProfile userprofile = db.UserProfiles.SingleOrDefault(p => p.UserName == name);
            HttpFileCollectionBase uploadFile = Request.Files;
            if (uploadFile.Count > 0)
            {
                video = uploadFile[0];
                System.IO.Stream inStream = video.InputStream;
                byte[] fileData = new byte[video.ContentLength];
                inStream.Read(fileData, 0, video.ContentLength);
                string filename = video.FileName;
                string test = HttpRuntime.AppDomainAppPath;

                string path = test + "\\Content\\Files\\";
                //System.IO.File.AppendAllText(@"C:\Users\HP User\Documents\GitHub\March Latest\Content\Files\uploadedList.txt", filename + "\r\n");
                //postedFile.SaveAs(@"C:\Users\HP User\Documents\GitHub\March Latest\Content\Files\" + postedFile.FileName);

                var directoryInfo = new DirectoryInfo(path);
                Course coursename = db.Courses.FirstOrDefault(p => p.CourseId == CourseId);

                if (directoryInfo.Exists)
                {
                    Console.WriteLine("Create a directory");
                    directoryInfo.CreateSubdirectory(email_id);

                    directoryInfo = new DirectoryInfo(path + "\\" + email_id);
                    string old_path = path + "\\" + email_id + "\\";

                    Console.WriteLine("Create a directory");
                    directoryInfo.CreateSubdirectory("Courses");

                    directoryInfo = new DirectoryInfo(old_path + "Courses\\");
                    string temp_path = old_path + "Courses\\";

                    Console.WriteLine("Create a directory");
                    directoryInfo.CreateSubdirectory(coursename.CourseName);

                    string new_path = temp_path + coursename.CourseName + "\\";
                    Console.WriteLine("New path", new_path);

                    System.IO.File.AppendAllText(old_path + "uploadedList.txt", filename + "\r\n");
                    System.IO.File.AppendAllText(new_path + "uploadedList.txt", filename + "\r\n");

                    video.SaveAs(new_path + video.FileName);

                }

            }

            //return View("Index", new { id = userprofile.UserProfileId });
            //return RedirectToAction("Index", "Profile");
            return RedirectToAction("CourseAddVideo", "Course", new { id = CourseId });
            //return View(userprofile);
        }

        //
        // GET: /Course/Edit/5

        public ActionResult Edit(int id)
        {
            Course course = db.Courses.Find(id);
            return View(course);
        }

        //
        // POST: /Course/Edit/5

        [HttpPost]
        public ActionResult Edit(Course course, FormCollection formCollection)
        {
            foreach (String key in formCollection.AllKeys)
            {
                switch (key)
                {
                    case "StartDate":
                        try
                        {
                            String[] get = formCollection.Get(key).Split(',');
                            DateTime dt = DateTime.Parse(get[get.Length-1]);
                            course.StartDate = (DateTime?)dt;
                        }
                        catch
                        {
                        }
                        break;
                    case "EndDate":
                        try
                        {
                            String[] get = formCollection.Get(key).Split(',');
                            DateTime dt = DateTime.Parse(get[get.Length - 1]);
                            course.EndDate = (DateTime?)dt;
                        }
                        catch
                        {
                        }
                        break;
                    default:
                        break;

                }
            }

            if (ModelState.IsValid)
            {
                db.Entry(course).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", "Course", new { id = course.CourseId });
            }
            return View(course);
        }

        //
        // GET: /Course/Delete/5

        public ActionResult Delete(int id)
        {
            Course course = db.Courses.Find(id);
            return View(course);
        }

        //
        // POST: /Course/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Course course = db.Courses.Find(id);
            db.Courses.Remove(course);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        //
        // GET: /Course/Delete/5


        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}