using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SignalR.Hubs;
using SignalR.Chat.Models;

namespace SignalR.Chat.Controllers
{
    public class ChatController : Controller
    {
        //
        // GET: /Chat/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Loginuser(SignalR.Chat.Models.Client chatter)
        {
            return View("ChatWindow", chatter);
        }

    }
}
