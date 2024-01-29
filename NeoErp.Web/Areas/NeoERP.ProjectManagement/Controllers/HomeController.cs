using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NeoERP.ProjectManagement.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult DashBoard()
        {
            return View();
        }

        public ActionResult Preference()
        {
            return View();
        }
        public ActionResult Index()
        {
            return View();
        }
    }
}