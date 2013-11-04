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
    public class FavouritesController : Controller
    {
        FollowPeersDBEntities db = new FollowPeersDBEntities();
        FollowPeers.Models.FollowPeersDBEntities followPeersDB = new FollowPeers.Models.FollowPeersDBEntities();
        //
        // GET: /Favourites/
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Index()
        {
            return View();
        }

        //
        // POST: /Favourites/
        [ActionName("Index")]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Index(string Category, string keyword, string FavrMode)
        {
            string name = Membership.GetUser().UserName;
            UserProfile user = db.UserProfiles.SingleOrDefault(p => p.UserName == name);
            if (FavrMode != null && FavrMode != "")
            {
                ViewBag.FavMode = "true";
                //Do nothing
                if (Category == "" || Category == "All Categories")
                {
                    ViewBag.Search = "yes";
                    ViewBag.Category = "*Select All*";
                    ViewBag.Keyword = keyword;
                }
                else
                {
                    ViewBag.Search = "yes";
                    ViewBag.Category = Category;
                    ViewBag.Keyword = keyword;
                }
            }
            else
            {
                if (Category == "" || Category == "All Categories")
                {
                    ViewBag.Search = "yes";
                    ViewBag.Category = "*Select All*";
                    ViewBag.Keyword = keyword;
                }
                else
                {
                    ViewBag.Search = "yes";
                    ViewBag.Category = Category;
                    ViewBag.Keyword = keyword;
                }
            }
            /*if (AddToFav != null)
            {
                Favourite Item = new Favourite();
                Item.ItemLink = ItemLink;
                Item.ItemName = ItemName;
                Item.ItemType = Convert.ToInt32(ItemType);
                user.Favourites.Add(Item);

                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();

                ViewBag.FavouriteAdded = "true";
            }*/


            return View();
        }

        [HttpPost]
        public ActionResult AddFav(string AddLink, string AddName, string AddType, string AddTypeId)
        {
            string name = Membership.GetUser().UserName;
            UserProfile user = db.UserProfiles.SingleOrDefault(p => p.UserName == name);
            if ((AddLink != "" && AddLink != null) && (AddName != "" && AddName != null) && (AddType != "" && AddType != null) && (AddTypeId != "" && AddTypeId != null))
            {
                Favourite Item = new Favourite();
                Item.ItemLink = AddLink;
                Item.ItemName = AddName;
                Item.ItemType = Convert.ToInt32(AddType);
                Item.ItemTypeId = Convert.ToInt32(AddTypeId);

                //Add only if Favourite doesn't already exist
                Favourite FoundMatch = user.Favourites.FirstOrDefault(p => p.ItemTypeId == Item.ItemTypeId && p.ItemType == Item.ItemType);
                if (FoundMatch == null)
                {
                    user.Favourites.Add(Item);
                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();
                }

                ViewBag.FavouriteAdded = "true";
            }
            //var result = "Reached Controller";
            //return Json(result, JsonRequestBehavior.AllowGet);
            return RedirectToAction("Index", "Favourites");
        }

    }
}
