using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FollowPeers.Models;
using System.Data;
using System.Data.Entity;
using System.Web.Security;

namespace FollowPeers.Controllers
{
    public class TagsController : Controller
    {
        FollowPeersDBEntities db = new FollowPeersDBEntities();
        FollowPeers.Models.FollowPeersDBEntities followPeersDB = new FollowPeers.Models.FollowPeersDBEntities();
        string name = Membership.GetUser().UserName;
        static UserProfile myprofile;
        //
        // GET: /Tags/

        public ActionResult Index()
        {
            return View();
        }

        public void AddTag(Tag model)
        {
            myprofile = db.UserProfiles.SingleOrDefault(p => p.UserName == name);
            if (ModelState.IsValid)
            {
                // Add only if tag doesn't already exist
                Tag Check = db.Tags.FirstOrDefault(p => p.TagName == model.TagName);
                if (Check == null)
                {
                    db.Tags.Add(model);
                    db.Entry(model).State = EntityState.Modified;
                    db.SaveChanges();
                }
                return;
            }
        }

        public void AddTag(string TagName)
        {
            myprofile = db.UserProfiles.SingleOrDefault(p => p.UserName == name);
            if (ModelState.IsValid)
            {
                Tag Check = db.Tags.FirstOrDefault(p => p.TagName == TagName);
                if (Check == null)
                {
                    Tag item = new Tag();
                    item.TagName = TagName;

                    db.Tags.Add(item);
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                }
                return;
            }
        }

        [HttpPost]
        public JsonResult FindTags(string searchText, int maxResults)
        {
            var result = from n in followPeersDB.Tags
                         where ((n.TagName.ToLower().Contains(searchText.ToLower())))
                         orderby n.TagName
                         select new
                         {
                             value = n.TagId,
                             name = n.TagName,
                         };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult MyTags()
        {
            List<Tag> MyTagNames = new List<Tag>();
            List<Tag> PubTagNames = new List<Tag>();
            List<Tag> JobTagNames = new List<Tag>();
            myprofile = db.UserProfiles.SingleOrDefault(p => p.UserName == name);

            //Publication Tagnames used
            List<PublicationModel> MyPubs = new List<PublicationModel>();
            MyPubs = followPeersDB.PublicationModels.Where(p => p.UserProfile.UserProfileId == myprofile.UserProfileId).ToList();
            foreach (PublicationModel pub in MyPubs)
            {
                PubTagNames.AddRange(pub.Tags.Distinct().ToList());
            }
            foreach (Tag name in PubTagNames)
            {
                MyTagNames.Add(name);
            }

            //Jobs Tagnames used
            List<Jobs> MyJobs = new List<Jobs>();
            MyJobs = myprofile.Jobs.ToList();
            foreach (Jobs job in MyJobs)
            {
                JobTagNames.AddRange(job.Tags.Distinct().ToList());
            }
            foreach (Tag name in JobTagNames)
            {
                bool Check = MyTagNames.Any(TagItem => TagItem.TagId == name.TagId);
                if (Check == false)
                {
                    MyTagNames.Add(name);
                }
            }


            //Add the Final Tag Names
            MyTagNames = MyTagNames.Distinct().ToList();

            return View(MyTagNames);
        }

    }
}
