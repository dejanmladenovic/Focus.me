using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Focus_me.Entities;
using Cassandra;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace Focus_me.Controllers
{
    public class UsersController : Controller
    {
        // GET: Users
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Register()
        {
            return View();
        }
        [ValidateAntiForgeryToken]
        [System.Web.Mvc.HttpPost]
        public string RegisterNewUser([FromBody]User user)
        {
            ISession session = SessionManager.GetSession();

            if (session == null)
                return "Session error";

            HttpPostedFileBase user_image = Request.Files["user_image_blob"];
            
            byte[] user_image_blob = null;
            if (user_image != null)
            {
                if (user_image.ContentType != "image/jpeg")
                {
                    return "file format error";
                }
                user_image_blob = new byte[user_image.ContentLength];
                user_image.InputStream.Read(user_image_blob, 0, user_image.ContentLength);
            }
            if(user.profile_description == null)
            {
                user.profile_description = "";
            }
            var st = session.Prepare("insert into \"user\" (\"id\",user_image_blob, user_name, full_name, profile_description, user_password, number_of_followers, number_of_following, number_of_posts)" +
                                            "values (uuid(),?,?,?,?,?,0,0,0)");
            var statement = st.Bind(user_image_blob, user.user_name, user.full_name, user.profile_description, user.user_password);

            RowSet newUser = session.Execute(statement);
            if (newUser.IsFullyFetched)
            {
                return "success.";
            }
            else
            {
                return "Writing error.";
            }
        }
        [System.Web.Mvc.HttpGet]
        public ActionResult ViewUser(string user_name)
        {
            ISession session = SessionManager.GetSession();

            if (session == null)
                return null;

            var st = session.Prepare("select * from user where user_name = ?");
            var statement = st.Bind(user_name);

            Row u = session.Execute(statement).FirstOrDefault();
            if(u == null)
            {
                return null;
            }

            User user = new User()
            {
                id = (System.Guid)u["id"],
                full_name = u["full_name"].ToString(),
                image_blob = (byte[])u["image_blob"],
                user_name = u["user_name"].ToString(),
                profile_description = u["profile_description"].ToString(),
                number_of_followers = (int)u["number_of_followers"],
                number_of_following = (int)u["number_of_following"],
                number_of_posts = (int)u["number_of_posts"]
            };
            return View((object)user);
        }

        public ActionResult Login()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        [System.Web.Mvc.HttpPost]
        public string LoginIfValid()
        {
            ISession session = SessionManager.GetSession();

            if (session == null)
                return "DB error";

            string username = Request["user_name"];
            string password = Request["user_password"];
            var st = session.Prepare("select * from user where user_name = ? and user_password = ? ALLOW FILTERING");
            var statement = st.Bind(username, password);

            Row u = session.Execute(statement).FirstOrDefault();
            if (u == null)
            {
                return "error";
            }
            else
            {
                Session["user_name"] = u["user_name"];
                Session["user_id"] = u["id"];
                Session["user_image_blob"] = ((byte[])u["user_image_blob"]);
                return "success";
            }
        }

        
        
    }
}