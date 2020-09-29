using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApp.Controllers
{
    public class LogoutController : Controller
    {
        // GET: Logout
        [HttpGet]
        public ActionResult Index()
        {
            Session.Abandon();
            return Redirect("login");
        }
    }
}