using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace MESWeb
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            HostingEnvironment.RegisterObject(new MESWeb.Timers.BackgroundMessageServerTimer());
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Session["LastException"] = Server.GetLastError();
            Server.ClearError();
            //Response.Redirect("/Home/Error");
            Response.RedirectToRoute(new { Controller = "Home", Action = "Error" });
        }
    }
}
