﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FollowPeers.Models;
using System.Web.Security;
using System.Data;
using System.Data.Entity;
using System.Net.Mail;
using System.Net;
using System.Data.Objects;
using System.Data.SqlClient;
using System.Web.Helpers;
using System.IO;
using System.Data.Entity.Validation;

namespace FollowPeers.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        //
        // GET: /Profile/
        FollowPeersDBEntities followPeersDB = new FollowPeersDBEntities();
        static UserProfile myprofile;
        public ActionResult Index(int id, String message)
        {
            ViewData["message"] = message;
            string name = Membership.GetUser().UserName;
            UserProfile viewerprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserProfileId == id);
            myprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);

            if (myprofile == null) //if the database stores a wrong info about name
            {
                name = " " + name;
                myprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
                myprofile.UserName.Trim();
                followPeersDB.SaveChanges();
            }

            //my profile and first time viewing
            if (id == myprofile.UserProfileId && viewerprofile.firsttime == true)
            {

                return RedirectToAction("CompleteProfile", "Profile", new { id = viewerprofile.UserProfileId });

            }
            else
            {
                ViewBag.Name = viewerprofile.UserName;
                if ((followPeersDB.Relationships.SingleOrDefault(p => p.personAName == name && p.personBName == viewerprofile.UserName) != null))//already followed
                    ViewBag.Alreadyfollowed = 1;
                else
                    ViewBag.Alreadyfollowed = 0;

                return View(viewerprofile);
            }

        }
        [ChildActionOnly]
        public ActionResult JobRecommendation()
        {
            string name = Membership.GetUser().UserName;
            UserProfile myprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            string specializationname = " "; IEnumerable<Jobs> tempresult = null;
            if (myprofile.Specializations.Count() != 0)
            {
                specializationname = myprofile.Specializations.ElementAt(0).SpecializationName;
                Specialization spec = followPeersDB.Specializations.First(p => p.SpecializationName.Contains(specializationname));
                tempresult = from j in followPeersDB.Jobs
                             where ((j.Country == myprofile.Country) && (j.ownerID != myprofile.UserProfileId))
                             orderby j.Title
                             select j;

                List<Jobs> temp = new List<Jobs>();
                foreach (var r in tempresult)
                {
                    /* if (r.Specializations.Contains(spec))
                     {
                         temp.Add(r);
                     }*/
                }
                if (temp.Count() > 2) return PartialView(temp);
            }

            tempresult = from j in followPeersDB.Jobs
                         where ((j.Country == myprofile.Country) && (j.ownerID != myprofile.UserProfileId))
                         orderby j.Title
                         select j;


            return PartialView(tempresult);

        }

        public ActionResult ViewRecommendedJobs()
        {
            string name = Membership.GetUser().UserName;
            UserProfile myprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            string specializationname = " "; IEnumerable<Jobs> tempresult = null;
            if (myprofile.Specializations.Count() != 0)
            {
                specializationname = myprofile.Specializations.ElementAt(0).SpecializationName;
                Specialization spec = followPeersDB.Specializations.First(p => p.SpecializationName.Contains(specializationname));
                tempresult = from j in followPeersDB.Jobs
                             where ((j.Country == myprofile.Country) && (j.ownerID != myprofile.UserProfileId))
                             orderby j.Title
                             select j;

                List<Jobs> temp = new List<Jobs>();
                foreach (var r in tempresult)
                {
                    /*if (r.Specializations.Contains(spec))
                    {
                        temp.Add(r);
                    }*/
                }
                if (temp.Count() > 2) return View(temp);
            }

            tempresult = from j in followPeersDB.Jobs
                         where ((j.Country == myprofile.Country) && (j.ownerID != myprofile.UserProfileId))
                         orderby j.Title
                         select j;
            return View(tempresult);
        }


        [ChildActionOnly]
        public ActionResult Following(int id)
        {
            //string name = Membership.GetUser().UserName;

            UserProfile temp = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserProfileId == id);
            string name = temp.UserName;
            List<UserProfile> result = new List<UserProfile>();

            IQueryable<string> custQuery = from cust in followPeersDB.Relationships where cust.personAName == name select cust.personBName;

            foreach (string personBname in custQuery)
            {
                result.Add(followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == personBname));
            }

            ViewData["link"] = temp.UserName;
            return PartialView(result);
        }

        [ChildActionOnly]
        public ActionResult FollowedBy(int id)
        {
            //string name = Membership.GetUser().UserName;

            UserProfile temp = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserProfileId == id);
            string name = temp.UserName;
            List<UserProfile> result = new List<UserProfile>();

            IQueryable<string> custQuery = from cust in followPeersDB.Relationships where cust.personBName == name select cust.personAName;

            foreach (string personAname in custQuery)
            {
                result.Add(followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == personAname));
            }
            ViewData["link"] = temp.UserName;

            return PartialView(result);
        }

        [HttpGet]
        public ActionResult FollowedByList(string name)
        {
            return RedirectToAction("Index", "FollowedBy", new { id = 1 });
        }

        [HttpGet]
        public ActionResult FollowingList(string name)
        {
            return RedirectToAction("Index", "FollowedBy", new { id = 1 });
        }
        [ChildActionOnly]
        public ActionResult FollowPeopleRecommendation()
        {

            string myname = Membership.GetUser().UserName;
            FollowPeers.Models.UserProfile userprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == myname);
            string organization = userprofile.Organization;
            string department = userprofile.Departments;
            IEnumerable<UserProfile> result;
            List<UserProfile> resultlist = null;
            result = from UserProfile in followPeersDB.UserProfiles
                     where (UserProfile.Organization.Contains(organization) && UserProfile.activated == true)
                     orderby UserProfile.UserName
                     select UserProfile;
            if (result.Count() < 10)
            {
                result = from UserProfile in followPeersDB.UserProfiles
                         where ((UserProfile.Organization.Contains(organization) || (UserProfile.Departments.Contains(department))) && UserProfile.activated == true)
                         orderby UserProfile.UserName
                         select UserProfile;
            }

            if (result.Count() < 10)
            {
                result = from UserProfile in followPeersDB.UserProfiles
                         orderby UserProfile.UserName
                         select UserProfile;
            }
            resultlist = result.ToList();
            foreach (var r in result)
            {
                if ((followPeersDB.Relationships.SingleOrDefault(p => p.personAName == userprofile.UserName && p.personBName == r.UserName) != null)) //already followed
                {
                    resultlist.Remove(r);
                }
                if (r.UserProfileId == userprofile.UserProfileId)
                    resultlist.Remove(r);
            }

            return PartialView(resultlist);

        }
        [ChildActionOnly]
        public ActionResult ProgressTracker()
        {
            string name = Membership.GetUser().UserName;
            myprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            ViewBag.Percent = (myprofile.SignUpProgress * 100).ToString();
            if (myprofile.Organization == null) { ViewBag.Text = "Update Basic Info"; ViewBag.Link = Server.HtmlEncode("/Profile/Edit"); }
            else if (myprofile.PhotoUrl == "/Content/TempImages/default.jpg") { ViewBag.Text = "Upload Profile Photo"; ViewBag.Link = Server.HtmlEncode("/Profile/UploadPhoto"); }
            else if (myprofile.Specializations.Count() == 0) { ViewBag.Text = "Update Research Interests"; ViewBag.Link = Server.HtmlEncode("/Profile/EditResearch"); }
            else if (myprofile.Educations.Count() == 0) { ViewBag.Text = "Update Education History"; ViewBag.Link = Server.HtmlEncode("/Profile/EditEducation"); }
            else if (myprofile.Employments.Count() == 0) { ViewBag.Text = "Update Employment History"; ViewBag.Link = Server.HtmlEncode("/Profile/EditEmployment"); }
            else if (myprofile.Achievements.Count() == 0) { ViewBag.Text = "Update Achievements"; ViewBag.Link = Server.HtmlEncode("/Profile/EditAchievement"); }
            else if (myprofile.Contact.Email == null) { ViewBag.Text = "Update Contact Info"; ViewBag.Link = Server.HtmlEncode("/Profile/EditContact"); }

            return PartialView();
        }

        /// <summary>
        /// Bookmark Publication
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [ChildActionOnly]
        public ActionResult BookmarkPublication()
        {
            string name = Membership.GetUser().UserName;
            myprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);

            int numberOfBookmarks = myprofile.PublicationBookmark.Count();
            List<int> bookmarkID = myprofile.PublicationBookmark.ToList();
            ViewBag.infopresent = false;

            if (numberOfBookmarks == 1)
            {
                var result = from n in followPeersDB.PublicationModels
                             where n.publicationID.Equals(bookmarkID[0])
                             orderby n.title
                             select n;
                ViewBag.infopresent = true;
                return PartialView(result.ToList());
            }
            if (numberOfBookmarks == 2)
            {
                var result = from n in followPeersDB.PublicationModels
                             where (n.publicationID.Equals(bookmarkID[0]) || n.publicationID.Equals(bookmarkID[1]))
                             orderby n.title
                             select n;
                ViewBag.infopresent = true;
                return PartialView(result.ToList());
            }
            if (numberOfBookmarks == 3)
            {
                var result = from n in followPeersDB.PublicationModels
                             where (n.publicationID.Equals(bookmarkID[0]) || n.publicationID.Equals(bookmarkID[1]) || n.publicationID.Equals(bookmarkID[2]))
                             orderby n.title
                             select n;
                ViewBag.infopresent = true;
                return PartialView(result.ToList());
            }
            if (numberOfBookmarks == 4)
            {
                var result = from n in followPeersDB.PublicationModels
                             where (n.publicationID.Equals(bookmarkID[0]) || n.publicationID.Equals(bookmarkID[1]) || n.publicationID.Equals(bookmarkID[2]) || n.publicationID.Equals(bookmarkID[3]))
                             orderby n.title
                             select n;
                ViewBag.infopresent = true;
                return PartialView(result.ToList());
            }
            if (numberOfBookmarks == 5)
            {
                var result = from n in followPeersDB.PublicationModels
                             where (n.publicationID.Equals(bookmarkID[0]) || n.publicationID.Equals(bookmarkID[1]) || n.publicationID.Equals(bookmarkID[2]) || n.publicationID.Equals(bookmarkID[3]) || n.publicationID.Equals(bookmarkID[4]))
                             orderby n.title
                             select n;
                ViewBag.infopresent = true;
                return PartialView(result.ToList());
            }
            return PartialView();
        }

        public ActionResult CompleteProfile(string message)
        {
            if (message != null) ViewData["message"] = message;
            string name = Membership.GetUser().UserName;
            myprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            return View(myprofile);
        }



        public ActionResult PostJob(string message)
        {
            if (message != null) ViewData["message"] = message;
            string name = Membership.GetUser().UserName;
            myprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            return View(myprofile);
        }


        public ActionResult UploadPhoto()
        {
            string name = Membership.GetUser().UserName;
            myprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            return View(myprofile);
        }
        public ActionResult UploadFile()
        {
            string name = Membership.GetUser().UserName;
            myprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            return View(myprofile);
        }

        [HttpPost]
        public ActionResult UploadProfilePhoto(FormCollection formCollection)
        {
            string name = Membership.GetUser().UserName;
            UserProfile userprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);

            string path = HttpRuntime.AppDomainAppPath;
            string profilePicPath = path + "\\Content\\Files\\" + name + "\\ProfilePicture";

            var profileDir = new DirectoryInfo(profilePicPath);

            if (!profileDir.Exists)
            {
                System.IO.Directory.CreateDirectory(profilePicPath);
            }

            var image = WebImage.GetImageFromRequest();
            if (image != null)
            {
                if (image.Width > 500)
                {
                    image.Resize(500, ((500 * image.Height) / image.Width));
                }

                var filename = Path.GetFileName(image.FileName);
                string toSaveString = profilePicPath + "/" + filename;
                image.Save(toSaveString);
                string filePath = "~/Content/Files/" + name + "/ProfilePicture/" + filename;
                userprofile.PhotoUrl = Url.Content(filePath);

                CreateUpdates("Changed Profile Picture", "/Profile/Index/" + userprofile.UserProfileId, 1, userprofile.UserProfileId); //CreateUpdates(message,link,type)

                followPeersDB.Entry(userprofile).State = EntityState.Modified;
                followPeersDB.SaveChanges();
     
                ViewData["message"] = "Successfully updated profile photo";
                return RedirectToAction("Index", "Profile", new { message = "Successfully Updated Profile Photo", id = userprofile.UserProfileId });

            }

            return RedirectToAction("Index", "Profile", new { message = "Profile Photo could not be uploaded", id = userprofile.UserProfileId });
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

        public ActionResult Delete_File(string delPath)
        {
            string filename = Path.GetFileName(delPath);

            string directory = Path.GetDirectoryName(delPath);

            //if (new DirectoryInfo(directory).Exists)
            //{

            //    string list_path = directory + "\\uploadedList.txt";

            //    string[] lines = System.IO.File.ReadAllLines(list_path);
            //    List<string> lines_list = lines.ToList();
            //    List<string> lines_list_2 = lines.ToList();

            //    foreach (string line in lines_list)
            //    {
            //        if (line.Equals(filename))
            //            lines_list_2.Remove(line);
            //    }

            //    string[] newLines = lines_list_2.ToArray();

            //    if (newLines != null)
            //        System.IO.File.WriteAllLines(list_path, newLines);
            //    else
            //        System.IO.File.WriteAllText(list_path, "");
            //}

            System.IO.File.Delete(delPath);

            return RedirectToAction("UploadFile", "Profile", null);
        }

        [HttpPost]
        public ActionResult UploadFile(FormCollection formCollection)
        {
            string name = Membership.GetUser().UserName;
            string email_id = Membership.GetUser().Email;
            UserProfile userprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            HttpFileCollectionBase uploadFile = Request.Files;
            if (uploadFile.Count > 0)
            {
                HttpPostedFileBase postedFile = uploadFile[0];
                if (postedFile.FileName == "")
                {
                    return RedirectToAction("UploadFile", "Profile", null);
                }
                System.IO.Stream inStream = postedFile.InputStream;
                byte[] fileData = new byte[postedFile.ContentLength];
                inStream.Read(fileData, 0, postedFile.ContentLength);
                string filename = postedFile.FileName;
                string test = HttpRuntime.AppDomainAppPath;

                string path = test + "\\Content\\Files\\";
                //System.IO.File.AppendAllText(@"C:\Users\HP User\Documents\GitHub\March Latest\Content\Files\uploadedList.txt", filename + "\r\n");
                //postedFile.SaveAs(@"C:\Users\HP User\Documents\GitHub\March Latest\Content\Files\" + postedFile.FileName);

                var directoryInfo = new DirectoryInfo(path);

                if (directoryInfo.Exists)
                {
                    Console.WriteLine("Create a directory");
                    directoryInfo.CreateSubdirectory(email_id);

                    string new_path = path + email_id + "\\";
                    Console.WriteLine("New path", new_path);

                    System.IO.File.AppendAllText(new_path + "uploadedList.txt", filename + "\r\n");
                    postedFile.SaveAs(new_path + postedFile.FileName);

                }

            }

            //return View("Index", new { id = userprofile.UserProfileId });
            //return RedirectToAction("Index", "Profile");
            return RedirectToAction("UploadFile", "Profile", null);
            //return View(userprofile);

        }





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
                if (type == 5) { record2.type = 4; record2.owner = myprofile.UserProfileId; record2.Own = true; }
                UserProfile tempuserprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == personAname);
                if (record2.owner != tempuserprofile.UserProfileId && type == 5) { record2.Own = false; }
                tempuserprofile.Updates.Add(record2);
            } //add a new update record for the followers            
        }




        private void UpdateEducation(string[] Organization, string[] startYear, string[] endYear, string[] Degree, string[] Country, UserProfile userprofile)
        {
            //if new organization, add to the DB
            foreach (var org in Organization)
            {
                if ((followPeersDB.Organizations.SingleOrDefault(p => p.Name == org)) == null)
                {
                    followPeersDB.Organizations.Add(new Organization { Name = org });
                }

            }

            //removing education
            int count = userprofile.Educations.Count();
            for (int i = 0; i < count; i++)
            {
                Education tempEdu = userprofile.Educations.ElementAt(0);
                followPeersDB.Educations.Remove(tempEdu);
                //       userprofile.Educations.Remove(tempEdu);
            }

            //adding the education records
            for (int i = 0; i < Organization.Count(); i++)
            {
                if (Organization != null)
                {
                    string tempUni = Organization.ElementAt(i);
                    string tempStart = startYear.ElementAt(i);
                    string tempEnd = endYear.ElementAt(i);
                    string tempDegree = Degree.ElementAt(i);
                    string tempCountry = Country.ElementAt(i);

                    if (tempUni == null || tempUni == "" || tempUni == " ")
                    { }

                    else
                    {
                        Education tempEdu = new Education

                        {
                            UniversityName = tempUni,
                            startYear = Convert.ToInt16(tempStart),
                            endYear = Convert.ToInt16(tempEnd),
                            Degree = tempDegree,
                            country = tempCountry
                        };


                        userprofile.Educations.Add(tempEdu);
                        tempEdu.UserProfile = userprofile;
                    }
                }
            }
        }



        private void UpdateEmployment(string[] EmployerName, string[] startYear, string[] endYear, string[] Description, string[] Role, UserProfile userprofile)
        {
            foreach (var org in EmployerName)
            {
                if ((followPeersDB.Organizations.SingleOrDefault(p => p.Name == org)) == null)
                {
                    followPeersDB.Organizations.Add(new Organization { Name = org });
                }

            }

            int count = userprofile.Educations.Count();
            for (int i = 0; i < count; i++)
            {
                Employment tempEmp = userprofile.Employments.ElementAt(0);
                followPeersDB.Employments.Remove(tempEmp);
                //       userprofile.Educations.Remove(tempEdu);
            }

            if (EmployerName != null)
            {
                for (int i = 0; i < EmployerName.Count(); i++)
                {
                    if (EmployerName != null)
                    {
                        string tempEmployer = EmployerName.ElementAt(i);

                        string tempStart = startYear.ElementAt(i);
                        string tempEnd = endYear.ElementAt(i);
                        string tempDesc = Description.ElementAt(i);
                        string tempRole = Role.ElementAt(i);

                        Employment tempEmp = new Employment
                        {
                            EmployerName = tempEmployer,

                            startYear = Convert.ToInt16(tempStart),
                            endYear = Convert.ToInt16(tempEnd),
                            Role = tempRole,
                            Description = tempDesc
                        };
                        userprofile.Employments.Add(tempEmp);
                        tempEmp.UserProfile = userprofile;
                    }
                }
            }
        }

        private void UpdateAchievement(string[] Title, string[] Description, string[] startYear, string[] endYear, string[] Keyword, string[] link, UserProfile userprofile)
        {
            int count = userprofile.Achievements.Count();
            for (int i = 0; i < count; i++)
            {
                Achievement tempAch = userprofile.Achievements.ElementAt(0);
                followPeersDB.Achievements.Remove(tempAch);
                //       userprofile.Educations.Remove(tempEdu);
            }
            if (Title != null)
            {
                for (int i = 0; i < Title.Count(); i++)
                {
                    if (Title != null)
                    {
                        string tempTitle = Title.ElementAt(i);
                        string tempDescription = Description.ElementAt(i);
                        string tempStart = startYear.ElementAt(i);
                        string tempEnd = endYear.ElementAt(i);
                        string templink = link.ElementAt(i);

                        string tempKeyword = Keyword.ElementAt(i);
                        Achievement tempAch = new Achievement
                        {
                            UserProfileId = userprofile.UserProfileId,
                            Title = tempTitle,
                            Description = tempDescription,
                            startYear = null,
                            endYear = null,
                            Keyword = tempKeyword,
                            like = 0,
                            Link = templink

                        };
                        userprofile.Achievements.Add(tempAch);
                        tempAch.UserProfile = userprofile;
                    }
                }
            }
        }

        private void UpdatePortfolio(string[] Name, string[] Field, string[] Country, string[] Year, string[] Status, string[] Website, string[] MoreInfo, UserProfile userprofile)
        {
            int count = userprofile.Portfolios.Count();
            for (int i = 0; i < count; i++)
            {
                Portfolio tempCompany = userprofile.Portfolios.ElementAt(0);
                followPeersDB.Portfolios.Remove(tempCompany);
                //       userprofile.Educations.Remove(tempEdu);
            }
            if (Name != null)
            {
                for (int i = 0; i < Name.Count(); i++)
                {

                    string tempName = Name.ElementAt(i);
                    string tempField = Field.ElementAt(i);
                    string tempCountry = Country.ElementAt(i);
                    string tempYear = Year.ElementAt(i);
                    string tempStatus = Status.ElementAt(i);
                    string tempWebsite = Website.ElementAt(i);
                    string tempMoreInfo = MoreInfo.ElementAt(i);
                    Portfolio tempPort = new Portfolio
                    {
                        Name = tempName,
                        BusinessField = tempField,
                        Country = tempCountry,
                        Year = Convert.ToInt16(tempYear),
                        Status = tempStatus,
                        Website = tempWebsite,
                        MoreInfo = tempMoreInfo
                    };
                    userprofile.Portfolios.Add(tempPort);
                    tempPort.UserProfile = userprofile;
                }
            }
        }

        private void UpdateStudent(string[] Name, string[] Type, string[] StartYear, string[] EndYear, string[] About, string[] Link, UserProfile userprofile)
        {
            int count = userprofile.Students.Count();
            for (int i = 0; i < count; i++)
            {
                Student tempStu = userprofile.Students.ElementAt(0);
                followPeersDB.Students.Remove(tempStu);
                //       userprofile.Educations.Remove(tempEdu);
            }
            if (Name != null)
            {
                for (int i = 0; i < Name.Count(); i++)
                {
                    if (Name != null)
                    {
                        string tempName = Name.ElementAt(i);
                        string tempStart = StartYear.ElementAt(i);
                        string tempEnd = EndYear.ElementAt(i);
                        string tempType = Type.ElementAt(i);
                        string tempAbout = About.ElementAt(i);
                        string tempUserId = Link.ElementAt(i);

                        Student tempStu = new Student
                        {
                            Name = tempName,
                            Type = Convert.ToInt16(tempType),
                            StartYear = Convert.ToInt16(tempStart),
                            EndYear = Convert.ToInt16(tempEnd),
                            About = tempAbout,
                            Link = Convert.ToInt16(tempUserId)
                        };
                        userprofile.Students.Add(tempStu);
                        tempStu.UserProfile = userprofile;
                    }
                }
            }
        }

        private void UpdateSpecializations(int[] Specialization, UserProfile userprofile)
        {
            if (Specialization != null)
            {
                //specializations currently selected 
                var selectedSpecializations = new HashSet<int>(Specialization);

                //specializations already attached to user
                var existingSpecializations = new HashSet<int>
                    (userprofile.Specializations.Select(c => c.SpecializationId));

                //all specializations in the DB
                foreach (var s in followPeersDB.Specializations)
                {
                    //if the specialization is among those selected
                    if (selectedSpecializations.Contains(s.SpecializationId))
                    {
                        //and does not exit in the existing user specs, add it
                        if (!existingSpecializations.Contains(s.SpecializationId))
                        {
                            userprofile.Specializations.Add(s);
                            s.UserProfiles.Add(userprofile);
                        }
                    }
                    //if spec is not among those selected
                    else
                    {
                        //but was attached to this user, remove it
                        if (existingSpecializations.Contains(s.SpecializationId))
                        {
                            userprofile.Specializations.Remove(s);
                            Keyword temp = userprofile.Keywords.SingleOrDefault(p => p.Area == s.SpecializationName);
                            userprofile.Keywords.Remove(temp);
                            s.UserProfiles.Remove(userprofile);
                        }
                    }
                }

                int i = 0;
                foreach (var item in userprofile.Specializations)
                {
                    bool toadd = true;
                    if (userprofile.Keywords == null)
                    {
                        userprofile.Keywords = new List<Keyword>();
                    }
                    for (int j = 0; j < userprofile.Keywords.Count(); j++)
                    {
                        if (userprofile.Keywords.ElementAt(j).Area == userprofile.Specializations.ElementAt(i).SpecializationName)
                        { toadd = false; }
                    }
                    if (toadd == true)
                    {
                        userprofile.Keywords.Add(new Keyword { Area = userprofile.Specializations.ElementAt(i).SpecializationName });
                    }
                    i++;
                }


            }
        }

        public ActionResult ChangeTier(string id, string tier)
        {
            string myname = Membership.GetUser().UserName;
            Relationship targetRelationship = followPeersDB.Relationships.SingleOrDefault(p => p.personAName == id && p.personBName == myname);
            int Tier = Convert.ToInt16(tier);
            targetRelationship.tier = Tier;
            followPeersDB.SaveChanges();
            return RedirectToAction("TierFollowers", "Profile", null);
        }
        public ActionResult DetailJob(int id)
        {
            Jobs j = followPeersDB.Jobs.First(p => p.JobId == id);
            string name = Membership.GetUser().UserName;
            UserProfile userprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            string temp = j.JobId.ToString();
            if (userprofile.Jobs.Contains(j) && j.ownerID != userprofile.UserProfileId)//need to add a button
                ViewData[temp] = "1";
            else
                ViewData[temp] = "0";

            return View(j);
        }


        [HttpGet]
        public string searchajax(string val)
        {
            string myname = Membership.GetUser().UserName;
            UserProfile userprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == myname);

            IEnumerable<UserProfile> people = from a in followPeersDB.UserProfiles
                                              where ((a.FirstName.Contains(val) || (a.LastName.Contains(val) || (a.UserName.Contains(val)) && a.activated == true)))
                                              orderby a.FirstName
                                              select a;

            IEnumerable<Jobs> jobs = from j in followPeersDB.Jobs
                                     where (j.Title.Contains(val) || j.Description.Contains(val))
                                     orderby j.Title
                                     select j;

            IEnumerable<PublicationModel> publication = from p in followPeersDB.PublicationModels
                                                        where (p.title.Contains(val) || p.description.Contains(val) || p.keyword.Contains(val))
                                                        orderby p.title
                                                        select p;

            IEnumerable<PatentModel> patent = from pa in followPeersDB.PatentModels
                                              where (pa.Title.Contains(val) || pa.Keyword.Contains(val) || pa.About.Contains(val))
                                              orderby pa.Title
                                              select pa;

            IEnumerable<Organization> organizations = from o in followPeersDB.Organizations
                                                      where o.Name.Contains(val)
                                                      orderby o.Name
                                                      select o;

            IEnumerable<Specialization> specializations = from s in followPeersDB.Specializations
                                                          where (s.SpecializationName.Contains(val) || s.Field.Contains(val))
                                                          orderby s.SpecializationName
                                                          select s;
            IEnumerable<ForumTopic> forumtopics = from t in followPeersDB.ForumTopics
                                                  where (t.Name.Contains(val) || t.Description.Contains(val) || t.Category.Contains(val))
                                                  orderby t.Name
                                                  select t;

            string returnstring = "";
            int totalresults = 0;
            if (people.Count() != 0)
            {
                int i = 0;
                returnstring += "<div class='each_rec' style='height: 9px;background: #777;'><a href='../../Profile/Search?keywords=" + val + "' style='height: 12px;line-height: 7px;width:100%;color: #ddd;'>People</a></div>";
                foreach (var p in people)
                {
                    if (i == 4) { break; }
                    string organization = p.Organization;
                    //if (p.FirstName.Length + p.LastName.Length + p.Organization.Length > 46) { organization = p.Organization.Substring(0, (46 - (p.FirstName.Length + p.LastName.Length))); organization += "..."; }
                    returnstring += "<div class='each_rec'><a href='../../Profile/Index/" + p.UserProfileId + "' style='margin-top: -4px;height: 28px;line-height: 25px;width:100%;'>" + p.FirstName + " " + p.LastName + " (" + organization + ")" + "</a></div>";
                    i++;
                    totalresults++;
                }
            }

            if (publication.Count() != 0)
            {
                int i = 0;
                returnstring += "<div class='each_rec' style='height: 9px;background: #777;'><a href='../../PublicationModel/Search?searchstring=" + val + "' style='height: 12px;line-height: 7px;width:100%;color: #ddd;'>Publication</a></div>";
                foreach (var p in publication)
                {
                    if (i == 4) { break; }
                    returnstring += "<div class='each_rec'><a href='../../PublicationModel/Details/" + p.publicationID + "' style='margin-top: -4px;height: 28px;line-height: 25px;width:100%;'>" + p.title + " " + p.type + "</a></div>";
                    i++;
                    totalresults++;
                }
            }

            if (patent.Count() != 0)
            {
                int i = 0;
                returnstring += "<div class='each_rec' style='height: 9px;background: #777;'><a href='../../PatentModel/Search?searchstring=" + val + "' style='height: 12px;line-height: 7px;width:100%;color: #ddd;'>Patent</a></div>";
                foreach (var p in patent)
                {
                    if (i == 4) { break; }
                    returnstring += "<div class='each_rec'><a href='../../PatentModel/Details/" + p.ID + "' style='margin-top: -4px;height: 28px;line-height: 25px;width:100%;'>" + p.Title + "</a></div>";
                    i++;
                    totalresults++;
                }
            }

            if (organizations.Count() != 0)
            {
                int i = 0;
                returnstring += "<div class='each_rec' style='height: 9px;background: #777;'><a href='../../Profile/Search?organization=" + val + "' style='height: 12px;line-height: 7px;width:100%;color: #ddd;'>Organizations</a></div>";
                foreach (var o in organizations)
                {
                    if (i == 2) { break; }
                    returnstring += "<div class='each_rec'><a href='../../Profile/Search?organization=" + o.Name + "' style='margin-top: -4px;height: 28px;line-height: 25px;width:100%;'>" + o.Name + "</a></div>";
                    i++;
                    totalresults++;
                }
            }

            if (specializations.Count() != 0)
            {
                int i = 0;
                returnstring += "<div class='each_rec' style='height: 9px;background: #777;'><a href='../../Profile/Search?specialization=" + val + "' style='height: 12px;line-height: 7px;width:100%;color: #ddd;'>Research Interests</a></div>";
                foreach (var s in specializations)
                {
                    if (i == 2) { break; }
                    returnstring += "<div class='each_rec'><a href='../../Profile/Search?specialization=" + s.SpecializationName + "' style='margin-top: -4px;height: 28px;line-height: 25px;width:100%;'>" + s.SpecializationName + "</a></div>";
                    i++;
                    totalresults++;
                }
            }

            if (jobs.Count() != 0)
            {
                int i = 0;
                returnstring += "<div class='each_rec' style='height: 9px;background: #777;'><a href='../../Profile/SearchJobs?sort=1&keywords=" + val + "' style='height: 12px;line-height: 7px;width:100%;color: #ddd;'>Jobs</a></div>";
                foreach (var j in jobs)
                {
                    if (i == 2) { break; }
                    returnstring += "<div class='each_rec'><a href='../../Profile/DetailJob/" + j.JobId + "' style='margin-top: -4px;height: 28px;line-height: 25px;width:100%;'>" + j.Title + "</a></div>";
                    i++;
                    totalresults++;
                }
            }

            if (forumtopics.Count() != 0)
            {
                int i = 0;
                returnstring += "<div class='each_rec' style='height: 9px;background: #777;'><a href='../../Forum/Search?q=" + val + "' style='height: 12px;line-height: 7px;width:100%;color: #ddd;'>Forum Topics</a></div>";
                foreach (var t in forumtopics)
                {
                    if (i == 2) { break; }
                    returnstring += "<div class='each_rec'><a href='../../Forum/TopicDetail/?Id=" + t.ForumTopicId + "' style='margin-top: -4px;height: 28px;line-height: 25px;width:100%;'>" + t.Category + "</a></div>";
                    i++;
                    totalresults++;
                }
            }

            //if (totalresults == 0) { returnstring += "<div class='each_rec'><a href='#' style='height: 18px;line-height: 15px;'>No results found</as></div>"; }
            returnstring += "<div class='each_rec2'><a href='../../Profile/Search?keywords=" + val + "' style='margin-top: -4px;height: 28px;line-height: 25px;width:100%;'>View More Results (" + val + ")</a></div>";
            return returnstring;
        }
        public string MarkNotificationsasSeen()
        {
            string name = Membership.GetUser().UserName;
            myprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);

            foreach (var noti in myprofile.Notifications)
            {
                noti.New = false;
            }
            followPeersDB.SaveChanges();
            return "";
        }
        public ActionResult ViewMoreNotifications()
        {
            string name = Membership.GetUser().UserName;
            myprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            var result = myprofile.Notifications.OrderByDescending(x => x.NotificationID);
            return View(result);
        }
        public string GetNotifications()
        {
            string name = Membership.GetUser().UserName;
            myprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            string returnstring = "";
            int i = 0;
            var notificationslist = myprofile.Notifications.OrderByDescending(x => x.NotificationID);
            foreach (var noti in notificationslist)
            {
                if (i == 5) break;
                string msg = noti.message;
                if (noti.message.Length > 75) msg = noti.message.Substring(0, 70) + "...";
                returnstring += "<div class='each_rec'><div style='height: 50px;overflow: hidden;float: left;'><img src='" + noti.imagelink + "' style='width:50px;'></div><div style='float: left;width: 70%;text-align:left;'><a href='" + noti.link + "' style='height: 50px;line-height: 14px;width: 100%;font-weight: normal;'>" + msg + "</a></div><div style='clear: both;'></div></div>";
                i++;
            }

            if (myprofile.Notifications.Count() == 0)
            {
                returnstring += "<div class='each_rec'><div style='float: left;width: 95%;'><a href='#' style='height: 25px;line-height: 14px;width: 100%;font-weight: normal;'>You have no notifications.</a></div><div style='clear: both;'></div></div>";
            }
            if (notificationslist.Count() != 0) returnstring += "<div class='each_rec'><div style='float: left;width: 95%;'><a href='../../Profile/ViewMoreNotifications' style='height: 25px;line-height: 14px;width: 100%;font-weight: normal;'> View More Notifications </a></div><div style='clear: both;'></div></div>";
            return returnstring;
        }
        public string GetNumberofNewNoti()
        {
            int count = 0;
            string name = Membership.GetUser().UserName;
            myprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            foreach (var noti in myprofile.Notifications)
            {
                if (noti.New == true) { count++; }
            }
            if (count == 0) return "";
            return count.ToString();
        }
        public ActionResult ViewBookmarkJobs()
        {
            string name = Membership.GetUser().UserName;
            myprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            List<Jobs> result = myprofile.Jobs.ToList();
            List<Jobs> tempresult = myprofile.Jobs.ToList();
            foreach (var j in tempresult)
            {
                if (j.ownerID == myprofile.UserProfileId) { result.Remove(j); }
            }
            return View(result);
        }

        public ActionResult ViewBookmarkPublications()
        {
            string name = Membership.GetUser().UserName;
            myprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            IEnumerable<Bookmark> result = from b in followPeersDB.Bookmarks
                                           where (b.userID == myprofile.UserProfileId && b.bookmarkType == "Publication")
                                           select b;
            List<PublicationModel> tempresult = new List<PublicationModel>();
            foreach (var b in result)
            {
                PublicationModel temppublication = followPeersDB.PublicationModels.SingleOrDefault(p => p.publicationID == b.itemID);
                tempresult.Add(temppublication);
            }

            return View(tempresult);
        }

        public ActionResult ViewBookmarkPatents()
        {
            string name = Membership.GetUser().UserName;
            myprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            IEnumerable<Bookmark> result = from b in followPeersDB.Bookmarks
                                           where (b.userID == myprofile.UserProfileId && b.bookmarkType == "Patent")
                                           select b;
            List<PatentModel> tempresult = new List<PatentModel>();
            foreach (var b in result)
            {
                PatentModel temppublication = followPeersDB.PatentModels.SingleOrDefault(p => p.ID == b.itemID);
                tempresult.Add(temppublication);
            }

            return View(tempresult);
        }

        [HttpGet]
        public ActionResult Search(string keywords, string department, string organization, string specialization)
        {
            string myname = Membership.GetUser().UserName;
            UserProfile userprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == myname);

            if (organization != null)
            {
                if (department == null)
                {
                    IEnumerable<UserProfile> result = from UserProfile in followPeersDB.UserProfiles
                                                      where (UserProfile.Organization.Contains(organization) && UserProfile.activated == true)
                                                      orderby UserProfile.UserName
                                                      select UserProfile;
                    ViewBag.searchstring = "People from " + organization;
                    foreach (var u in result)
                    {
                        string temp = u.UserName;
                        if ((followPeersDB.Relationships.SingleOrDefault(p => p.personAName == userprofile.UserName && p.personBName == u.UserName) != null))//already followed
                            ViewData[temp] = "1";
                        else
                            ViewData[temp] = "0";
                    }

                    return View(result);
                }
                else
                {
                    IEnumerable<UserProfile> result = from UserProfile in followPeersDB.UserProfiles
                                                      where (UserProfile.Organization.Contains(organization) && UserProfile.Departments.Contains(department) && UserProfile.activated == true)
                                                      orderby UserProfile.UserName
                                                      select UserProfile;
                    ViewBag.searchstring = "People from " + department + ", " + organization;
                    foreach (var u in result)
                    {
                        string temp = u.UserName;
                        if ((followPeersDB.Relationships.SingleOrDefault(p => p.personAName == userprofile.UserName && p.personBName == u.UserName) != null))//already followed
                            ViewData[temp] = "1";
                        else
                            ViewData[temp] = "0";
                    }

                    return View(result);
                }

            }

            else if (department != null)
            {
                IEnumerable<UserProfile> result = from UserProfile in followPeersDB.UserProfiles
                                                  where (UserProfile.Departments.Contains(department) && UserProfile.activated == true)
                                                  orderby UserProfile.UserName
                                                  select UserProfile;
                ViewBag.searchstring = "People from " + department;
                foreach (var u in result)
                {
                    string temp = u.UserName;
                    if ((followPeersDB.Relationships.SingleOrDefault(p => p.personAName == userprofile.UserName && p.personBName == u.UserName) != null))//already followed
                        ViewData[temp] = "1";
                    else
                        ViewData[temp] = "0";
                }

                return View(result);
            }


            else if (keywords != null && keywords.Length != 0)
            {
                ViewData["keywords"] = keywords;
                IEnumerable<UserProfile> result = from UserProfile in followPeersDB.UserProfiles
                                                  where (UserProfile.UserName.Contains(keywords) || UserProfile.FirstName.Contains(keywords) || UserProfile.LastName.Contains(keywords)) && UserProfile.activated == true
                                                  orderby UserProfile.UserName
                                                  select UserProfile;
                ViewBag.searchstring = "People Search Results for " + keywords;
                foreach (var u in result)
                {
                    string temp = u.UserName;
                    if ((followPeersDB.Relationships.SingleOrDefault(p => p.personAName == userprofile.UserName && p.personBName == u.UserName) != null))//already followed
                        ViewData[temp] = "1";
                    else
                        ViewData[temp] = "0";
                }

                return View(result);
            }

            else if (specialization != null)
            {
                IEnumerable<UserProfile> result = from UserProfile in followPeersDB.UserProfiles
                                                  where (UserProfile.Specializations.Any(i => i.SpecializationName.Contains(specialization)) && UserProfile.activated == true)
                                                  orderby UserProfile.UserName
                                                  select UserProfile;
                ViewBag.searchstring = "People with research interest in " + specialization;
                foreach (var u in result)
                {
                    string temp = u.UserName;
                    if ((followPeersDB.Relationships.SingleOrDefault(p => p.personAName == userprofile.UserName && p.personBName == u.UserName) != null))//already followed
                        ViewData[temp] = "1";
                    else
                        ViewData[temp] = "0";
                }

                return View(result);
            }
            else
            {
                IEnumerable<UserProfile> result = from UserProfile in followPeersDB.UserProfiles
                                                  orderby UserProfile.UserName
                                                  select UserProfile;
                ViewBag.searchstring = "Showing all people within the network";
                foreach (var u in result)
                {
                    string temp = u.UserName;
                    if ((followPeersDB.Relationships.SingleOrDefault(p => p.personAName == userprofile.UserName && p.personBName == u.UserName) != null))//already followed
                        ViewData[temp] = "1";
                    else
                        ViewData[temp] = "0";
                }
                return View(result);
            }

        }
        [HttpPost]
        public void Follow(string username, string url)
        {
            string name = Membership.GetUser().UserName;
            myprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            // UserProfile followerProfile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == username);
            // UserProfile followerProfile = new UserProfile();

            Relationship tempR = new Relationship
            {
                personAName = name,
                personBName = username,
                tier = myprofile.Default //setting the default tier
            };

            followPeersDB.Relationships.Add(tempR);
            followPeersDB.SaveChanges();

            UserProfile personB = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == username);
            UserProfile follower = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            Notification newnoti = new Notification
            {
                message = follower.FirstName + " started following you.",
                link = "/Profile/Index/" + follower.UserProfileId,
                New = true,
                imagelink = myprofile.PhotoUrl,
            };

            personB.Notifications.Add(newnoti);
            followPeersDB.Entry(personB).State = EntityState.Modified;
            followPeersDB.SaveChanges();


            Response.Redirect(url);
            // return RedirectToAction("Index", "Profile", new { id = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == username).UserProfileId });

        }


        [HttpPost]
        public void Bookmark(string jobid, string url)
        {
            string name = Membership.GetUser().UserName;
            myprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            int jobidINT = Convert.ToInt16(jobid);
            // UserProfile followerProfile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == username);
            // UserProfile followerProfile = new UserProfile();
            Jobs job = followPeersDB.Jobs.SingleOrDefault(p => p.JobId == jobidINT);
            myprofile.Jobs.Add(job);
            followPeersDB.SaveChanges();

            UserProfile updateowner = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserProfileId == job.ownerID);
            string jobdescription = job.Description;
            if (jobdescription.Length > 60) jobdescription = job.Description.Substring(0, 60) + "...";
            Notification newnoti = new Notification
            {
                message = myprofile.FirstName + " bookmarked your job post : " + jobdescription,
                link = "/Profile/Index/" + myprofile.UserProfileId,
                New = true,
                imagelink = myprofile.PhotoUrl,
            };

            updateowner.Notifications.Add(newnoti);
            followPeersDB.Entry(updateowner).State = EntityState.Modified;
            followPeersDB.SaveChanges();

            Response.Redirect(url);
            // return RedirectToAction("Index", "Profile", new { id = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == username).UserProfileId });

        }

        [HttpPost]
        public void UnBookmark(string jobid, string url)
        {
            string name = Membership.GetUser().UserName;
            myprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            int jobidINT = Convert.ToInt16(jobid);
            // UserProfile followerProfile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == username);
            // UserProfile followerProfile = new UserProfile();
            Jobs job = followPeersDB.Jobs.SingleOrDefault(p => p.JobId == jobidINT);
            myprofile.Jobs.Remove(job);
            followPeersDB.SaveChanges();
            Response.Redirect(url);
            // return RedirectToAction("Index", "Profile", new { id = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == username).UserProfileId });

        }
        public ActionResult CreateGroup(string name, string[] members)
        {
            string myname = Membership.GetUser().UserName;
            myprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == myname);
            Group grp = new Group
            {
                Name = name,
                OwnerId = myprofile.UserProfileId,
                Members = new List<UserProfile>()
            };
            if (members == null || members.Count() == 0) return RedirectToAction("Index", "Bulletin", new { sort = 3 });
            for (int i = 0; i < members.Count(); i++)
            {
                string tempstr = members[i];

                IQueryable<UserProfile> custQuery = from cust in followPeersDB.UserProfiles where cust.FirstName + " " + cust.LastName == tempstr select cust;
                foreach (var profile in custQuery)
                {
                    grp.Members.Add(profile);
                }

            }
            followPeersDB.Groups.Add(grp);
            followPeersDB.SaveChanges();
            return RedirectToAction("Index", "Bulletin", new { sort = 3 });

        }
        public ActionResult RemoveFromGrp(string name, string grpname)
        {
            int id = Convert.ToInt16(name);
            string myname = Membership.GetUser().UserName;
            myprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == myname);
            UserProfile profile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserProfileId == id);

            Group grp = followPeersDB.Groups.SingleOrDefault(p => p.Name == grpname & p.OwnerId == myprofile.UserProfileId); //come back here

            grp.Members.Remove(profile);
            if (grp.Members.Count() == 0)
                followPeersDB.Groups.Remove(grp);
            //profile.Groups.Remove(grp);
            followPeersDB.SaveChanges();
            return RedirectToAction("Index", "Bulletin", new { sort = 3 });

        }

        public ActionResult AddMembertoGroup(string name, string grpname)
        {
            string myname = Membership.GetUser().UserName;
            myprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == myname);
            UserProfile profile = followPeersDB.UserProfiles.SingleOrDefault(p => p.FirstName + " " + p.LastName == name);

            IQueryable<Group> custQuery = from cust in followPeersDB.Groups where cust.OwnerId == myprofile.UserProfileId select cust;
            foreach (var grp in custQuery)
            {
                if (grp.Members.Contains(profile))
                {
                    if (grp.Name == grpname) { return RedirectToAction("Index", "Bulletin", new { sort = 3 }); }
                    grp.Members.Remove(profile);
                }
                if (grp.Members.Count() == 0)
                    followPeersDB.Groups.Remove(grp);
            }

            Group Togrp = followPeersDB.Groups.SingleOrDefault(p => p.OwnerId == myprofile.UserProfileId & p.Name == grpname); //come back here

            Togrp.Members.Add(profile);

            followPeersDB.SaveChanges();
            return RedirectToAction("Index", "Bulletin", new { sort = 3 });

        }

        public ActionResult DeleteGroup(string name)
        {
            string myname = Membership.GetUser().UserName;
            myprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == myname);
            Group grp = followPeersDB.Groups.SingleOrDefault(p => p.Name == name & p.OwnerId == myprofile.UserProfileId);

            followPeersDB.Groups.Remove(grp);
            followPeersDB.SaveChanges();
            return RedirectToAction("Index", "Bulletin", new { sort = 3 });

        }
        [HttpPost]
        public void UnFollow(string username, string url)
        {
            string name = Membership.GetUser().UserName;
            myprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            // UserProfile followerProfile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == username);
            // UserProfile followerProfile = new UserProfile();

            //  Relationship tempR = followPeersDB.Relationships.SingleOrDefault(p=> p.personAName == name && p.personBName==username);

            followPeersDB.Relationships.Remove(followPeersDB.Relationships.SingleOrDefault(p => p.personAName == name && p.personBName == username));
            followPeersDB.SaveChanges();

            UserProfile personB = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == username);
            UserProfile follower = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            Notification newnoti = new Notification
            {
                message = follower.FirstName + " stopped following you.",
                link = "/Profile/Index/" + follower.UserProfileId,
                New = true,
                imagelink = myprofile.PhotoUrl,
            };

            personB.Notifications.Add(newnoti);
            followPeersDB.Entry(personB).State = EntityState.Modified;
            followPeersDB.SaveChanges();


            Response.Redirect(url);
            //return RedirectToAction("Index", "Profile", new { id = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == username).UserProfileId });

        }

        [HttpPost]
        public void like(string username, int achievementid, string url)
        {


            string name = Membership.GetUser().UserName;
            myprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            UserProfile liked = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == username);
            UserProfile likedby = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            Achievement ach = followPeersDB.Achievements.SingleOrDefault(p => p.AchievementId == achievementid);
            if ((followPeersDB.AchievementLikes.Where(p => p.AchievementId == achievementid && p.UserProfileId == likedby.UserProfileId).ToList().Count() == 0))
            {

                ach.like++;
                followPeersDB.Entry(ach).State = EntityState.Modified;

                AchievementLike achlike = new AchievementLike
                {
                    UserProfileId = likedby.UserProfileId,
                    AchievementId = ach.AchievementId,
                    UserProfile = likedby

                };
                followPeersDB.Entry(achlike).State = EntityState.Added;
                Notification newnoti = new Notification
                {
                    message = likedby.FirstName + " liked your achievement titled" + ach.Title,
                    link = "/Profile/Index/" + likedby.UserProfileId,
                    New = true,
                    imagelink = likedby.PhotoUrl,
                };

                liked.Notifications.Add(newnoti);
                followPeersDB.Entry(liked).State = EntityState.Modified;

                Update newupdate = new Update
                {

                    New = true,
                    Own = true,
                    Time = System.DateTime.Now,
                    message = @liked.FirstName + "," + @ach.Title,
                    link = "/Profile/Index/" + liked.UserProfileId,
                    owner = likedby.UserProfileId,
                    type = 10 // achievement like
                };

                liked.Updates.Add(newupdate);
                followPeersDB.Entry(liked).State = EntityState.Modified;

                followPeersDB.SaveChanges();



            }
            else
            {
                ach.like--;
                followPeersDB.Entry(ach).State = EntityState.Modified;
                followPeersDB.SaveChanges();
                followPeersDB.AchievementLikes.Remove(followPeersDB.AchievementLikes.SingleOrDefault(p => p.AchievementId == achievementid && p.UserProfileId == likedby.UserProfileId));
                followPeersDB.SaveChanges();
            }

            Response.Redirect(url);
        }


        public ActionResult Info()
        {
            return PartialView();
        }

        public ActionResult NoticeBoard()
        {
            return PartialView();
        }

        public ActionResult Publications()
        {
            return PartialView();
        }

        public ActionResult Patents()
        {
            return PartialView();
        }

        //
        // GET: /Profile/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }



        //
        // GET: /Profile/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Profile/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        internal List<Organization> FindOrganization(string searchText, int maxResults)
        {
            var result = from n in followPeersDB.Organizations
                         where n.Name.Contains(searchText)
                         orderby n.Name
                         select n;

            return result.Take(maxResults).ToList();

        }

        [HttpPost]
        public JsonResult FindOrganizationNames(string searchText, int maxResults)
        {
            var result = FindOrganization(searchText, maxResults);
            return Json(result);
        }




        internal List<Department> FindDepartment(string searchText, int maxResults)
        {


            var result = from n in followPeersDB.Departments
                         where n.Name.Contains(searchText)
                         orderby n.Name
                         select n;

            return result.Take(maxResults).ToList();

        }

        [HttpPost]
        public JsonResult FindDepartmentNames(string searchText, int maxResults)
        {
            var result = FindDepartment(searchText, maxResults);
            return Json(result);
        }

        [HttpPost]
        public JsonResult FindUserNames(string searchText, int maxResults)
        {
            var result = from n in followPeersDB.UserProfiles
                         where ((n.FirstName.Contains(searchText)) || (n.LastName.Contains(searchText)) || ((n.FirstName + " " + n.LastName).Contains(searchText)))
                         orderby n.FirstName
                         select new
                         {
                             FirstName = n.FirstName,
                             LastName = n.LastName,
                             UserProfileId = n.UserProfileId,
                             PhotoUrl = n.PhotoUrl,
                             Organization = n.Organization,
                             UserName = n.UserName
                         };
            return Json(result);

        }


        public JsonResult FindSpecializations(string q)
        {
            var result = from n in followPeersDB.Specializations
                         where n.SpecializationName.Contains(q)
                         orderby n.SpecializationName
                         select new
                         {
                             value = n.SpecializationId,
                             name = n.SpecializationName,

                         };
            return Json(result, JsonRequestBehavior.AllowGet);

        }

        public JsonResult FindFriends(string q)
        {
            var result = from n in followPeersDB.UserProfiles
                         where ((n.FirstName.Contains(q)) || (n.LastName.Contains(q)) || ((n.FirstName + " " + n.LastName).Contains(q)))
                         orderby n.FirstName
                         select new
                         {
                             value = n.UserProfileId,
                             name = n.FirstName + " " + n.LastName,

                         };
            return Json(result, JsonRequestBehavior.AllowGet);

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

        public ActionResult TierFollowers()
        {
            string name = Membership.GetUser().UserName;
            myprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);

            return View(myprofile);
        }

        public ActionResult Privacy()
        {
            string name = Membership.GetUser().UserName;
            myprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);

            return View(myprofile);
        }

        [HttpPost]
        public ActionResult Privacy(int? Email1, int? Email2, int? Email3, int? Phone1, int? Phone2, int? Phone3, int? Mobile1, int? Mobile2, int? Mobile3, int? Address1, int? Address2, int? Address3, int? Education1, int? Education2, int? Education3, int? Publication1, int? Publication2, int? Publication3, int? Patent1, int? Patent2, int? Patent3, int? Noticeboard1, int? Noticeboard2, int? Noticeboard3, int? Aboutme1, int? Aboutme2, int? Aboutme3, int Default)
        {
            string name = Membership.GetUser().UserName;
            UserProfile userprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            if (TryUpdateModel(userprofile, "", null, new string[] { "Specializations" }))
            {
                if (Email1 != null) userprofile.Tiers.ElementAt(0).Email = 1;
                else userprofile.Tiers.ElementAt(0).Email = 0;
                if (Email2 != null) userprofile.Tiers.ElementAt(1).Email = 1;
                else userprofile.Tiers.ElementAt(1).Email = 0;
                if (Email3 != null) userprofile.Tiers.ElementAt(2).Email = 1;
                else userprofile.Tiers.ElementAt(2).Email = 0;

                if (Phone1 != null) userprofile.Tiers.ElementAt(0).Phone = 1;
                else userprofile.Tiers.ElementAt(0).Phone = 0;
                if (Phone2 != null) userprofile.Tiers.ElementAt(1).Phone = 1;
                else userprofile.Tiers.ElementAt(1).Phone = 0;
                if (Phone3 != null) userprofile.Tiers.ElementAt(2).Phone = 1;
                else userprofile.Tiers.ElementAt(2).Phone = 0;

                if (Mobile1 != null) userprofile.Tiers.ElementAt(0).Mobile = 1;
                else userprofile.Tiers.ElementAt(0).Mobile = 0;
                if (Mobile2 != null) userprofile.Tiers.ElementAt(1).Mobile = 1;
                else userprofile.Tiers.ElementAt(1).Mobile = 0;
                if (Mobile3 != null) userprofile.Tiers.ElementAt(2).Mobile = 1;
                else userprofile.Tiers.ElementAt(2).Mobile = 0;

                if (Address1 != null) userprofile.Tiers.ElementAt(0).Address = 1;
                else userprofile.Tiers.ElementAt(0).Address = 0;
                if (Address2 != null) userprofile.Tiers.ElementAt(1).Address = 1;
                else userprofile.Tiers.ElementAt(1).Address = 0;
                if (Address3 != null) userprofile.Tiers.ElementAt(2).Address = 1;
                else userprofile.Tiers.ElementAt(2).Address = 0;

                if (Education1 != null) userprofile.Tiers.ElementAt(0).Education = 1;
                else userprofile.Tiers.ElementAt(0).Education = 0;
                if (Education2 != null) userprofile.Tiers.ElementAt(1).Education = 1;
                else userprofile.Tiers.ElementAt(1).Education = 0;
                if (Education3 != null) userprofile.Tiers.ElementAt(2).Education = 1;
                else userprofile.Tiers.ElementAt(2).Education = 0;

                if (Publication1 != null) userprofile.Tiers.ElementAt(0).Publication = 1;
                else userprofile.Tiers.ElementAt(0).Publication = 0;
                if (Publication2 != null) userprofile.Tiers.ElementAt(1).Publication = 1;
                else userprofile.Tiers.ElementAt(1).Publication = 0;
                if (Publication3 != null) userprofile.Tiers.ElementAt(2).Publication = 1;
                else userprofile.Tiers.ElementAt(2).Publication = 0;

                if (Patent1 != null) userprofile.Tiers.ElementAt(0).Patent = 1;
                else userprofile.Tiers.ElementAt(0).Patent = 0;
                if (Patent2 != null) userprofile.Tiers.ElementAt(1).Patent = 1;
                else userprofile.Tiers.ElementAt(1).Patent = 0;
                if (Patent3 != null) userprofile.Tiers.ElementAt(2).Patent = 1;
                else userprofile.Tiers.ElementAt(2).Patent = 0;

                if (Noticeboard1 != null) userprofile.Tiers.ElementAt(0).Noticeboard = 1;
                else userprofile.Tiers.ElementAt(0).Noticeboard = 0;
                if (Noticeboard2 != null) userprofile.Tiers.ElementAt(1).Noticeboard = 1;
                else userprofile.Tiers.ElementAt(1).Noticeboard = 0;
                if (Noticeboard3 != null) userprofile.Tiers.ElementAt(2).Noticeboard = 1;
                else userprofile.Tiers.ElementAt(2).Noticeboard = 0;

                if (Aboutme1 != null) userprofile.Tiers.ElementAt(0).AboutMe = 1;
                else userprofile.Tiers.ElementAt(0).AboutMe = 0;
                if (Aboutme2 != null) userprofile.Tiers.ElementAt(1).AboutMe = 1;
                else userprofile.Tiers.ElementAt(1).AboutMe = 0;
                if (Aboutme3 != null) userprofile.Tiers.ElementAt(2).AboutMe = 1;
                else userprofile.Tiers.ElementAt(2).AboutMe = 0;

                userprofile.Default = Default;

                followPeersDB.Entry(userprofile).State = EntityState.Modified;
                followPeersDB.SaveChanges();
                return RedirectToAction("Index", "Notice", new { id = userprofile.UserProfileId });
            }

            if (userprofile.FirstName == null) ModelState.AddModelError("", "First Name cannot be left blank.");
            if (userprofile.LastName == null) ModelState.AddModelError("", "Last Name cannot be left blank.");

            return View(userprofile);
        }


        [HttpPost]
        public ActionResult UpdateStatus(string message, bool Ischecked)
        {
            bool check = Ischecked;
            //bool check = Convert.ToBoolean(Ischecked.Substring(0, Ischecked.IndexOf(',')));
            string name = Membership.GetUser().UserName;
            List<FollowPeers.Models.Relationship> followednames = followPeersDB.Relationships.Where(p => p.personAName == name).ToList();
            List<FollowPeers.Models.UserProfile> followedpeers = followPeersDB.UserProfiles.Where(p => p.UserProfileId < 0).ToList(); // null list
            List<FollowPeers.Models.UserProfile> taggedpeers = followPeersDB.UserProfiles.Where(p => p.UserProfileId < 0).ToList();
            foreach (var peername in followednames)
            {
                followedpeers.Add(followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == peername.personBName));
            }
            UserProfile userprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            userprofile.StatusMessage = message;
            string[] token = message.Split(' '); // extracting words from sentence
            // bool Ischecked = bool.Parse(Request.Form.GetValues("Ischecked")[0]);
            for (int i = 0; i < token.Count(); i++)
            {
                for (int j = 0; j < followedpeers.Count(); j++)
                {
                    if (token[i].StartsWith("@") && (token[i].Skip(1).Equals(followedpeers[j].FirstName) || token[i].Skip(1).Equals(followedpeers[j].LastName)))
                    {
                        taggedpeers.Add(followedpeers[j]); // adding only tagges peers

                    }
                }

            }

            if (check == true) //only status update and broadcast
            {
                CreateUpdates(message, "/Notice/Index/" + userprofile.UserProfileId, 9, userprofile.UserProfileId); // broadcast message type = 9 
                PosttoNoticeBoard(message, userprofile.UserProfileId.ToString(), check);
            }
            /*else
                if(check == true && taggedpeers.Count > 0) //status update broadcast and peer tagging
                {
                    foreach (var peer in taggedpeers)
                    {
                        CreateUpdates(message,  peer.UserProfileId.ToString(), 8, userprofile.UserProfileId); // broadcast message type = 8 
                    }
                    CreateUpdates(message, "/Notice/Index/" + userprofile.UserProfileId, 9, userprofile.UserProfileId); // broadcast message type = 9 
                    PosttoNoticeBoard(message, userprofile.UserProfileId.ToString(), check);
                }
            else
                if( check == false && taggedpeers.Count() > 0) // only status update and peer tagging
                {
                CreateUpdates(message, "/Notice/Index/" + userprofile.UserProfileId, 2, userprofile.UserProfileId);
                
                    foreach(var peer in taggedpeers)
                    {
                    CreateUpdates(message, peer.UserProfileId.ToString() + "," + peer.FirstName + " " + peer.LastName, 8, userprofile.UserProfileId);
                    PosttoNoticeBoard(message, peer.UserProfileId.ToString());
                    }
                }*/
            else // only status update
            {
                CreateUpdates(message, "/Notice/Index/" + userprofile.UserProfileId, 2, userprofile.UserProfileId);
            }
            followPeersDB.Entry(userprofile).State = EntityState.Modified;
            followPeersDB.SaveChanges();
            return RedirectToAction("Index", "Profile", new { id = userprofile.UserProfileId });
        }


        [HttpPost]
        public ActionResult PosttoNoticeBoard(string message, string redirect, bool Ischecked = false)
        {
            string name = Membership.GetUser().UserName;
            UserProfile userprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            // userprofile.StatusMessage = message;
            int id = Convert.ToInt16(redirect);
            // bool checkvalue = bool.Parse(Request.Form.GetValues("Ischecked")[0]);
            if (Ischecked == true)
            {
                int i = 0;
                List<FollowPeers.Models.Relationship> followednames = followPeersDB.Relationships.Where(p => p.personAName == name).ToList();
                List<FollowPeers.Models.UserProfile> followedpeers = followPeersDB.UserProfiles.Where(p => p.UserProfileId < 0).ToList(); // null list
                foreach (var peername in followednames)
                {

                    followedpeers.Add(followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == peername.personBName));
                    if (followedpeers.Count() > 0)
                    {
                        CreateUpdates(message, "/Notice/Index/" + followedpeers[i].UserProfileId, 9, followedpeers[i].UserProfileId);

                        /*  UserProfile Noticeowner = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserProfileId == id);
                          if (message.Length > 30) message = message.Substring(0, 25) + "...";

                          Notification newnoti = new Notification
                          {
                              message = userprofile.FirstName + " wrote on your Noticeboard : " + message,
                              link = "/Notice/Index/" + id,
                              New = true,
                              imagelink = userprofile.PhotoUrl,
                          };
                          Noticeowner.Notifications.Add(newnoti);
                          followPeersDB.Entry(Noticeowner).State = EntityState.Modified;
                          followPeersDB.SaveChanges();*/

                    }
                    i++;
                    followPeersDB.Entry(userprofile).State = EntityState.Modified;
                    followPeersDB.SaveChanges();
                }

                foreach (var peer in followedpeers)
                {
                    UserProfile Noticeowner = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserProfileId == peer.UserProfileId);
                    if (message.Length > 30) message = message.Substring(0, 25) + "...";

                    Notification newnoti = new Notification
                    {
                        message = userprofile.FirstName + " wrote on your Noticeboard : " + message,
                        link = "/Notice/Index/" + peer.UserProfileId,
                        New = true,
                        imagelink = userprofile.PhotoUrl,
                    };
                    Noticeowner.Notifications.Add(newnoti);
                    followPeersDB.Entry(Noticeowner).State = EntityState.Modified;
                    followPeersDB.SaveChanges();
                }

            }
            else
            {
                CreateUpdates(message, "/Notice/Index/" + id, 5, id);

                followPeersDB.Entry(userprofile).State = EntityState.Modified;
                followPeersDB.SaveChanges();

                UserProfile Noticeowner = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserProfileId == id);
                if (message.Length > 30) message = message.Substring(0, 25) + "...";

                Notification newnoti = new Notification
                {
                    message = userprofile.FirstName + " wrote on your Noticeboard : " + message,
                    link = "/Notice/Index/" + id,
                    New = true,
                    imagelink = userprofile.PhotoUrl,
                };
                Noticeowner.Notifications.Add(newnoti);
                followPeersDB.Entry(Noticeowner).State = EntityState.Modified;
                followPeersDB.SaveChanges();
            }


            return RedirectToAction("Index", "Profile", new { id = id });
        }


        [HttpPost]
        public ActionResult AddComment(int commentid, string message, int redirect)
        {
            string name = Membership.GetUser().UserName;
            UserProfile myprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);

            Update update = followPeersDB.Updates.SingleOrDefault(p => p.UpdateId == commentid);
            // UserProfile user = update.UserProfiles.ElementAt(0);
            //if (myprofile.UserProfileId != update.owner)
            if (myprofile.UserProfileId != update.UserProfiles.ElementAt(0).UserProfileId)
            {
                CreateUpdates(message, "/Notice/Index/" + myprofile.UserProfileId, 3, myprofile.UserProfileId);
                followPeersDB.Entry(myprofile).State = EntityState.Modified;
            }

            NoticeComment comment = new NoticeComment() { commenter = myprofile, message = message, time = DateTime.Now, update = update };
            followPeersDB.NoticeComments.Add(comment);

            followPeersDB.SaveChanges();

            //when adding a comment on a noticeboard, need to create a notification item also

            //notification items have to be created for 
            // 1. owner of the update the comment is attached to (if owner is not the same as current commenter)
            // 2. commenters of other comments under the same update (if they are not the same as current commenter)
            // 3. owner of the noticeboard (if owner is not the same as current commenter)

            List<UserProfile> notiaddedlist = new List<UserProfile>();
            //Case 1 owner of the update
            if (update.owner != myprofile.UserProfileId)
            {
                UserProfile updateowner = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserProfileId == update.owner);
                UserProfile user = update.UserProfiles.ElementAt(0);
                Notification newnoti = new Notification
                {
                    message = myprofile.FirstName + " commented on your post : " + message,
                    link = "/Notice/Index/" + user.UserProfileId,
                    New = true,
                    imagelink = myprofile.PhotoUrl,
                };
                var toadd = true;
                //to prevent adding notifications to the same person twice
                foreach (var item in notiaddedlist)
                {
                    if (item == updateowner) toadd = false;
                }
                if (toadd == true)
                {
                    notiaddedlist.Add(updateowner);
                    updateowner.Notifications.Add(newnoti);
                    followPeersDB.Entry(updateowner).State = EntityState.Modified;
                    followPeersDB.SaveChanges();
                }
            }

            //Case 2. commenters of other comments under the same update (if they are not the same as current commenter OR the noticeboard owner)
            IEnumerable<NoticeComment> result = from n in followPeersDB.NoticeComments
                                                where n.update.UpdateId == update.UpdateId
                                                select n;
            int noticeownerid = update.UserProfiles.ElementAt(0).UserProfileId;
            foreach (var n in result)
            {
                if ((n.commenterId != myprofile.UserProfileId) && (n.commenterId != noticeownerid))
                {
                    UserProfile commenter = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserProfileId == n.commenterId);
                    UserProfile user = update.UserProfiles.ElementAt(0);
                    Notification newnoti = new Notification
                    {
                        message = myprofile.FirstName + " commented on your post : " + message,
                        link = "/Notice/Index/" + user.UserProfileId,
                        New = true,
                        imagelink = myprofile.PhotoUrl,
                    };
                    var toadd = true;
                    //to prevent adding notifications to the same person twice
                    foreach (var item in notiaddedlist)
                    {
                        if (item == commenter) toadd = false;
                    }
                    if (toadd == true)
                    {
                        notiaddedlist.Add(commenter);
                        commenter.Notifications.Add(newnoti);
                        followPeersDB.Entry(commenter).State = EntityState.Modified;
                        followPeersDB.SaveChanges();
                    }
                }
            }
            //Case 3. owner of the noticeboard (if owner is not the same as current commenter)
            if (noticeownerid != myprofile.UserProfileId)
            {

                UserProfile noticeowner = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserProfileId == noticeownerid);
                Notification newnoti = new Notification
                {
                    message = myprofile.FirstName + " commented on your Noticeboard : " + message,
                    link = "/Notice/Index/" + noticeownerid,
                    New = true,
                    imagelink = myprofile.PhotoUrl,
                };
                var toadd = true;
                //to prevent adding notifications to the same person twice
                foreach (var item in notiaddedlist)
                {
                    if (item == noticeowner) toadd = false;
                }
                if (toadd == true)
                {
                    notiaddedlist.Add(noticeowner);
                    noticeowner.Notifications.Add(newnoti);
                    followPeersDB.Entry(noticeowner).State = EntityState.Modified;
                    followPeersDB.SaveChanges();
                }
            }

            return RedirectToAction("Index", "Profile", new { id = redirect });
        }


        public ActionResult UpdateKeywords(string keywordlist)
        {
            string myname = Membership.GetUser().UserName;
            myprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == myname);

            int count = myprofile.Keywords.Count();
            for (int i = 0; i < count; i++)
            {
                Keyword temp = myprofile.Keywords.ElementAt(0);
                myprofile.Keywords.Remove(temp);
                followPeersDB.Keywords.Remove(temp);

            }
            char[] delimiterChars = { ',' };
            string[] words = keywordlist.Split(delimiterChars);

            foreach (string s in words)
            {
                string s2 = s.Trim();
                Keyword temp = new Keyword { Area = s2 };
                myprofile.Keywords.Add(temp);
            }
            followPeersDB.Entry(myprofile).State = EntityState.Modified;
            followPeersDB.SaveChanges();
            return RedirectToAction("Index", "News", new { reader = "Yahoo" });
        }

        [HttpPost]
        public ActionResult UploadPhotoFile(IEnumerable<HttpPostedFileBase> files)
        {
            string name = Membership.GetUser().UserName;
            string email_id = Membership.GetUser().Email;
            UserProfile userprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            //HttpFileCollectionBase uploadFile = Request.Files;

            if (files != null && files.Count() > 0)
            {
                foreach (var uploadFile in files)
                {

                    //int filesToUpload = uploadFile.Count;
                    //while (filesToUpload > 0)
                    //{

                    //}

                    HttpPostedFileBase postedFile = uploadFile;
                    if (postedFile.FileName == "")
                    {
                        return RedirectToAction("Index", "Profile", new { message="The filename is invalid" , id = userprofile.UserProfileId });
                    }

                    var splitstring = postedFile.FileName.Split('.');
                    var ext = splitstring[splitstring.Length - 1];
                    ext = ext.ToLower();

                    switch (ext)
                    {
                        case "jpg":
                            break;
                        case "png":
                            break;
                        case "jpeg":
                            break;
                        default:
                            return RedirectToAction("Index", "Profile", new { message="The file format is unrecognized", id = userprofile.UserProfileId });
                    }


                    System.IO.Stream inStream = postedFile.InputStream;
                    byte[] fileData = new byte[postedFile.ContentLength];
                    inStream.Read(fileData, 0, postedFile.ContentLength);
                    string filename = postedFile.FileName;
                    string test = HttpRuntime.AppDomainAppPath;

                    string path = test + "\\Content\\Files\\";
                    //System.IO.File.AppendAllText(@"C:\Users\HP User\Documents\GitHub\March Latest\Content\Files\uploadedList.txt", filename + "\r\n");
                    //postedFile.SaveAs(@"C:\Users\HP User\Documents\GitHub\March Latest\Content\Files\" + postedFile.FileName);

                    var directoryInfo = new DirectoryInfo(path);

                    if (directoryInfo.Exists)
                    {
                        Console.WriteLine("Create a directory");
                        directoryInfo.CreateSubdirectory(email_id);



                        string toSave_path = path + email_id + "\\Photos\\";
                        Console.WriteLine("toSave path", toSave_path);

                        var checkPhotoDir = new DirectoryInfo(toSave_path);
                        if (!checkPhotoDir.Exists)
                        {
                            System.IO.Directory.CreateDirectory(toSave_path);
                        }

                        //System.IO.File.AppendAllText(toSave_path + "uploadedList.txt", filename + "\r\n");
                        postedFile.SaveAs(toSave_path + postedFile.FileName);

                    }
                }

            }

            //return View("Index", new { id = userprofile.UserProfileId });
            return RedirectToAction("Index", "Profile", new {message="Photo was successfully uploaded", id = userprofile.UserProfileId });
            //return RedirectToAction("UploadPhoto", "Profile", null);
            //return View(userprofile);

        }


        [HttpPost, ValidateInput(false)]
        public ActionResult EditProfile(FormCollection formCollection)
        {
            List<String> organizationStrings = new List<String>();
            List<String> countryStrings = new List<String>();
            List<String> degreeStrings = new List<String>();
            List<String> startYearStrings = new List<String>();
            List<String> endYearStrings = new List<String>();

            string name = Membership.GetUser().UserName;
            UserProfile userprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);

            //set first time view to false
            userprofile.firsttime = false;

            //birthday is still an issue here
            foreach (String key in formCollection.AllKeys)
            {
                switch (key)
                {
                    case "FirstName":
                        if (formCollection.Get(key) == null)
                        {
                            ModelState.AddModelError("", "First Name cannot be left blank.");
                        }
                        userprofile.FirstName = formCollection.Get(key);
                        break;
                    case "LastName":
                        userprofile.LastName = formCollection.Get(key);
                        break;
                    case "Gender":
                        userprofile.Gender = formCollection.Get(key);
                        break;
                    case "Status":
                        userprofile.Status = formCollection.Get(key);
                        break;
                    case "Birthday":

                        String[] bdayString = formCollection.Get(key).Split(',');
                        String bdate = bdayString[bdayString.Length - 1];
                        try { userprofile.Birthday = DateTime.Parse(bdate); }
                        catch (FormatException e)
                        {

                        }
                        break;
                    case "AboutMe":
                        userprofile.AboutMe = formCollection.Get(key);
                        break;
                    case "Contact.Street1":
                        userprofile.Contact.Street1 = formCollection.Get(key);
                        break;
                    case "Contact.Street2":
                        userprofile.Contact.Street2 = formCollection.Get(key);
                        break;
                    case "Contact.City":
                        userprofile.Contact.City = formCollection.Get(key);
                        break;
                    case "Contact.Country":
                        userprofile.Contact.Country = formCollection.Get(key);
                        break;
                    case "Contact.Mobile":
                        userprofile.Contact.Mobile = formCollection.Get(key);
                        break;
                    case "Contact.Phone":
                        userprofile.Contact.Phone = formCollection.Get(key);
                        break;
                    case "Contact.Fax":
                        userprofile.Contact.Fax = formCollection.Get(key);
                        break;
                    case "Contact.Email":
                        userprofile.Contact.Email = formCollection.Get(key);
                        break;
                    case "Contact.Website":
                        userprofile.Contact.Website = formCollection.Get(key);
                        break;

                    case "Organization":
                        foreach (String s in formCollection.Get(key).Split(','))
                        {
                            organizationStrings.Add(s);
                        }

                        break;
                    case "Country":
                        foreach (String s in formCollection.Get(key).Split(','))
                        {
                            countryStrings.Add(s);
                        }
                        break;
                    case "Degree":
                        foreach (String s in formCollection.Get(key).Split(','))
                        {
                            degreeStrings.Add(s);
                        }
                        break;
                    case "startYear":
                        foreach (String s in formCollection.Get(key).Split(','))
                        {
                            startYearStrings.Add(s);
                        }
                        break;
                    case "endYear":
                        foreach (String s in formCollection.Get(key).Split(','))
                        {
                            endYearStrings.Add(s);
                        }
                        break;


                    case "Company":
                        userprofile.Organization = formCollection.Get(key);
                        //if this is a new organization
                        if ((followPeersDB.Organizations.Where(p => p.Name == userprofile.Organization)) == null)
                        {
                            followPeersDB.Organizations.Add(new Organization { Name = userprofile.Organization });
                        }
                        break;
                    case "Departments":
                        userprofile.Departments = formCollection.Get(key);
                        //if a user adds a new Department
                        if ((followPeersDB.Departments.Where(p => p.Name == userprofile.Departments)) == null)
                        {
                            followPeersDB.Departments.Add(new Department { Name = userprofile.Departments });
                        }
                        break;

                    case "Specialization":
                        String[] specStringArray = formCollection.Get(key).Split(',');
                        int[] specIntArray = new int[specStringArray.Length];
                        int i = 0;
                        foreach (String s in specStringArray)
                        {
                            specIntArray[i] = Int32.Parse(s);
                            i++;
                        }
                        UpdateSpecializations(specIntArray, userprofile);
                        break;
                    default:
                        break;
                }

            }

            UpdateEducation(organizationStrings.ToArray(), startYearStrings.ToArray(), endYearStrings.ToArray(), degreeStrings.ToArray(), countryStrings.ToArray(), userprofile);


            CreateUpdates("Profile information updated.", "/Profile/Index/" + userprofile.UserProfileId, 1, userprofile.UserProfileId); //CreateUpdates(message,link,type)
            followPeersDB.Entry(userprofile).State = EntityState.Modified;
            try
            {
                followPeersDB.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                return RedirectToAction("Index", "Profile", new { message = "Profile Information Not Updated", id = userprofile.UserProfileId });
            }

            return RedirectToAction("Index", "Profile", new { message = "Profile Information Successfully Updated", id = userprofile.UserProfileId });



            //if (userprofile.FirstName == null) ModelState.AddModelError("", "First Name cannot be left blank.");
            //if (userprofile.LastName == null) ModelState.AddModelError("", "Last Name cannot be left blank.");
            //ModelState.AddModelError("", "Update Failed");


            //return View(userprofile);


        }


    }
}