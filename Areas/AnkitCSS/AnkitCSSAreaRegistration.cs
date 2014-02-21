using System.Web.Mvc;

namespace FollowPeers.Areas.AnkitCSS
{
    public class AnkitCSSAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "AnkitCSS";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "AnkitCSS_default",
                "AnkitCSS/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
