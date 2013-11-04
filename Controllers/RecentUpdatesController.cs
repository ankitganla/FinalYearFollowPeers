using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using FollowPeers.Models;
using System.Web.Security;


namespace FollowPeers.Controllers
{
    public class RecentUpdatesController : Controller
    {
        
        //
        // GET: /RecentUpdates/
        private FollowPeersDBEntities db = new FollowPeersDBEntities();

        public ActionResult RecentUpdates(Update update, UserProfile profile)
        {

            var peerupdate = (from p in db.Updates where p.owner!= 1 select p);

            return View(peerupdate);
        }

    }
}
