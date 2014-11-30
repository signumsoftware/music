using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Signum.Entities.Authorization;
using Signum.Engine.Dashboard;
using Signum.Web.Dashboard;
using Signum.Utilities;
using Signum.Engine;
using Signum.Web;

namespace Music.Web
{
    [AuthenticationRequired(false)]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (UserEntity.Current == null)
                return View();

            var panel = DashboardLogic.GetHomePageDashboard();
            if (panel != null)
                return View(DashboardClient.ViewPrefix.FormatWith("Dashboard"), panel);
            else
                return View();
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
