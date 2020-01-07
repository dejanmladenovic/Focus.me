using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Focus_me.Entities;
using Cassandra;

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

        [System.Web.Mvc.HttpPost]
        public string RegisterNewUser([FromBody]User user)
        {
            ISession session = SessionManager.GetSession();

            if (session == null)
                return "Problem u kreiranju sesije";

            string newUserId = System.Guid.NewGuid().ToString();
            HttpPostedFileBase user_image = Request.Files["user_image"];
            byte[] user_image_blob = null;
            if (user_image != null)
            {
                user_image_blob = new byte[user_image.ContentLength];
                user_image.InputStream.Read(user_image_blob, 0, user_image.ContentLength);
            }

            var st = session.Prepare("insert into \"user\" (\"id\",image_blob, user_name, full_name, profile_description, user_password, number_of_followers, number_of_following, number_of_posts)" +
                                            "values (?,?,?,?,?,?,0,0,0)");
            var statement = st.Bind(newUserId, user_image_blob, user.user_name, user.full_name, user.profile_description, user.user_password);

            RowSet newUser = session.Execute(statement);
            if (newUser.IsFullyFetched)
            {
                Session["user_id"] = newUserId;
                return "Uspesno upisan korisnik.";
            }
            else
            {
                return "Doslo je do greske kod upisa.";
            }
        }
        [System.Web.Mvc.HttpGet]
        public ActionResult ViewUser(string username)
        {
            ISession session = SessionManager.GetSession();

            if (session == null)
                return null;

            var st = session.Prepare("select * from user where id = ?");
            var statement = st.Bind(username);

            Row u = session.Execute(statement).FirstOrDefault();
            if(u == null)
            {
                return null;
            }

            User user = new User()
            {
                id = u["id"].ToString(),
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

        [System.Web.Mvc.HttpGet]
        public ActionResult LoadProfile(string id)
        {
            ISession session = SessionManager.GetSession();

            if(session == null)
                return null;

            var st = session.Prepare("select * from user where id = ?");
            var statement = st.Bind(id);
            Row u = session.Execute(statement).FirstOrDefault();
            if (u == null)
            {
                return null;
            }

            User user = new User()
            {
                id = u["id"].ToString(),
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
    }
}