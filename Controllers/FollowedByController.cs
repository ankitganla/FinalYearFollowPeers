using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using FollowPeers.Models;
using System.Data;
using System.Data.Entity;

namespace FollowPeers.Controllers
{
    public class FollowedByController : Controller
    {
        //
        // GET: /FollowedBy/
        FollowPeersDBEntities followPeersDB = new FollowPeersDBEntities();

        public ActionResult Index(int id)
        {
            List<UserProfile> result = new List<UserProfile>();
            List<UserProfile> tempresult = new List<UserProfile>();
            string myname = Membership.GetUser().UserName;
            IQueryable<string> custQuery = from cust in followPeersDB.Relationships where cust.personBName == myname select cust.personAName;
            IQueryable<string> custQuery2 = from cust in followPeersDB.Relationships where cust.personAName == myname select cust.personBName;
            List<string> duplicates = new List<string>();
            foreach (string personBname in custQuery2)
            {//who the person follows
                result.Add(followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == personBname));
                ViewData[personBname] = "1";
            }
            foreach (string personAname in custQuery)
            {//who is following the person
                result.Add(followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == personAname));
                ViewData[personAname] = "0";
            }
            foreach (string person1 in custQuery)
            {
                foreach (string person2 in custQuery2)
                {
                    if (person1 == person2)
                    {
                        ViewData[person1] = "2";
                        duplicates.Add(person1);
                    }
                }
            }

            foreach (string name in duplicates)
            {
                result.Remove(followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name));
            }
            ViewData["link"] = myname;
            ViewData["myself"] = 1;
            ViewBag.sort = id;
            return View(result);
        }

        public ActionResult ViewPage1()
        {
            return View();
        }

        public ActionResult MarkCommentAsRead(int id)
        {
            string name = Membership.GetUser().UserName;
            UserProfile myprofile = followPeersDB.UserProfiles.SingleOrDefault(p => p.UserName == name);
            foreach (var item in myprofile.Updates)
            {
                if (item.owner == id)
                {
                    item.New = false; //set the update as viewed
                    followPeersDB.Entry(item).State = EntityState.Modified;
                    followPeersDB.SaveChanges();
                }
            }
            return RedirectToAction("Index", "FollowedBy", new { id = myprofile.UserProfileId });
        }

    }
}
