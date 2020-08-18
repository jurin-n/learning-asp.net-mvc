using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApp.Common;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Index()
        {
            return View(new User() { UserId = "", Password = "", isValid = true }) ;
        }

        [HttpPost]
        public ActionResult Index(User user)
        {
            if (ModelState.IsValid)
            {
                var result = Authentication.IdAndPasswordSignIn(user.UserId, user.Password);
                if (result == 0)
                {
                    //認証成功した場合
                    Session["UserId"] = user.UserId;
                    return Redirect("order");
                }
            }
            user.isValid = false;
            return View(user);
        }
    }
}