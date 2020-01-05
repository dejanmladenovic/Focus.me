using Focus_me.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Focus_me.Controllers
{
    public class TestController : Controller
    {

        public ActionResult Test()
        {
            var user = new User();

            user.full_name = "Milos Stankovic";

            return View(user);
        }
    }
}