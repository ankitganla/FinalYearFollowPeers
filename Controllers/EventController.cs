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
    public class EventController : Controller
    {
        FollowPeersDBEntities db = new FollowPeersDBEntities();
        FollowPeers.Models.FollowPeersDBEntities followPeersDB = new FollowPeers.Models.FollowPeersDBEntities();
        string name = Membership.GetUser().UserName;
        static UserProfile myprofile;
        //
        // GET: /Event/

        public ActionResult Index()
        {
            myprofile = db.UserProfiles.SingleOrDefault(p => p.UserName == name);
            try
            {
                var result = from m in db.Events
                             where (m.Invitees.Any(p => p.UserProfileId == myprofile.UserProfileId))
                             orderby m.EventDate, m.EventTime
                             select m;

                return View(result.ToList());
            }
            catch
            {
                return View();
            }

        }


        //
        // GET: /Event/AddEvent

        public ActionResult AddEvent()
        {
            return View();
        }

        public ActionResult EventDetail(int id)
        {
            Event select = followPeersDB.Events.FirstOrDefault(p => p.EventId == id);
            return View(select);
        }

        
        public ActionResult EventEdit(int id)
        {
            Event select = followPeersDB.Events.FirstOrDefault(p => p.EventId == id);
            return View(select);
        }

        [HttpPost]
        public ActionResult EventDetail(int id, String Join, String Decline)
        {
            string name = Membership.GetUser().UserName;
            myprofile = db.UserProfiles.SingleOrDefault(p => p.UserName == name);
            EventUserProfileJoin JoinEvent = new EventUserProfileJoin();
            JoinEvent.EventId = id;
            JoinEvent.UserProfileId = myprofile.UserProfileId;
            if (Join != null)
            {
                JoinEvent.JoinStatus = 1;
            }
            else if (Decline != null)
            {
                JoinEvent.JoinStatus = 0;
            }
            db.EventUserProfileJoins.Add(JoinEvent);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        //
        // POST: /Event/EventEdit
        [HttpPost]
        public ActionResult EventEdit(Event model)
        {
            /*Event select = followPeersDB.Events.FirstOrDefault(p => p.EventId == model.EventId);
            /*if (model.EventTime == DateTime.MinValue)
            {
                //Assign Event its old time value in this case
                model.EventTime = select.EventTime;
            }
            select.EventName = model.EventName;
            select.EventDate = model.EventDate;
            select.EventTime = model.EventTime;
            select.EventDesc = model.EventDesc;

            db.Entry(select).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("EventDetail", new { id = model.EventId });*/

            if (ModelState.IsValid)
            {
                Event select = followPeersDB.Events.FirstOrDefault(p => p.EventId == model.EventId);
                select.EventName = model.EventName;
                select.EventDate = model.EventDate;
                select.EventTime = model.EventTime;
                select.EventDesc = model.EventDesc;

                db.Entry(select).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("EventDetail", new { id = model.EventId });
            }
            return RedirectToAction("EventDetail", new { id = model.EventId });

        }

        //
        // POST: /Event/AddEvent
        [HttpPost]
        public ActionResult AddEvent(Event model, string[] InvitesId)
        {
                if (model.EventName == null)
                {
                    model.EventName = "default";
                }
                if (model.EventDate == DateTime.MinValue)
                {
                    model.EventDate = DateTime.Now;
                }
                if (model.EventTime == DateTime.MinValue)
                {
                    model.EventTime = DateTime.Now;
                }
                if (model.EventDesc == null)
                {
                    model.EventDesc = "default";
                }

                /*
                //Adding Tags
                //To keep track of tags being added
                List<Tag> AddedTags = new List<Tag>();

                //Adding the Tag
                if (model.EventDesc!= null)
                {
                    string EventDescTags = model.EventDesc.ToString();
                    //If written new tag
                    string[] Tags = EventDescTags.Split(' ');
                    foreach (string tagname in Tags)
                    {
                        string Trimtagname = tagname.Trim();
                        Tag Item = followPeersDB.Tags.FirstOrDefault(p => p.TagName.ToLower() == Trimtagname.ToLower());
                        if (Item != null)
                        {
                            if (AddedTags.Contains(Item) != true && !(Item.Events.Any(p => p.EventId == model.EventId)))
                            {
                                Item.TagLinkedItems += 1;
                                model.Tags.Add(Item);
                                Item.Events.Add(model);
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
                            model.Tags.Add(NewItem);
                            NewItem.Events.Add(model);
                            AddedTags.Add(NewItem);
                        }
                    }
                }
                //-----------End of Adding Tags*/

                string name = Membership.GetUser().UserName;
                UserProfile user = db.UserProfiles.SingleOrDefault(p => p.UserName == name);

                //Adding event for User (Creator of Event) and Adding User to Event
                user.Events.Add(model);
                model.Invitees.Add(user);

                //Adding event for All invitees and all invitees to event
                if (InvitesId != null)
                {
                    foreach (var InviteeId in InvitesId)
                    {
                        if (InviteeId != null && InviteeId != "")
                        {
                            int id = Convert.ToInt32(InviteeId);
                            UserProfile invitee = db.UserProfiles.SingleOrDefault(p => p.UserProfileId == id);

                            try
                            {
                                if (user.UserProfileId != id)
                                {
                                    invitee.Events.Add(model);
                                    model.Invitees.Add(invitee);


                                    //For Notifications
                                    string makeMessage = user.FirstName + " invited you to an event : '" + model.EventName+"'";
                                    int eventnumber = followPeersDB.Events.Count() + 1;
                                    Notification newnoti = new Notification
                                    {
                                        message = makeMessage,
                                        link = "/Event/EventDetail/" + eventnumber,
                                        New = true,
                                    };

                                    invitee.Notifications.Add(newnoti);
                                }

                            }
                            catch
                            {
                            }
                        }
                    }
                }

                //Add event to database
                db.Events.Add(model);

                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();

                //Calculating Event Id to Create Notification
                int EventId = followPeersDB.Events.Count();

                //If Event was created, we find it and create a notification
                CategoryPost post = new CategoryPost
                {
                    TimeStamp = DateTime.Now,
                    PostMessage = "Created a new Event titled " + model.EventName,
                    Type = 3,
                    Postedby = user.UserProfileId,
                    Category = model.EventDesc,
                    Link = "/Event/EventDetail/" + EventId,
                };
                user.CategoryPosts.Add(post);

                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
        }

        [HttpPost]
        public JsonResult FindInviteeNames(string searchText, int maxResults)
        {
            //var result = FindInvitees(searchText, maxResults);
            //return Json(result);

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
    }
}