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
            var st = session.Prepare("insert into \"user\" (user_image_blob, user_name, full_name, profile_description, user_password)" +
                                            "values (?,?,?,?,?)");
            
            var statement = st.Bind(user_image_blob, user.user_name.ToLower(), user.full_name, user.profile_description, user.user_password);


            var st3 = session.Prepare("UPDATE user_stats SET number_of_followers = number_of_followers + 0, number_of_following = number_of_following + 0, number_of_posts = number_of_posts + 0 " +
                                        "WHERE user_name = ?");

            var statement3 = st3.Bind(user.user_name.ToLower());

            var batch = new BatchStatement()
                .Add(statement);

            RowSet newUser = session.Execute(batch);
            RowSet updateResult = session.Execute(statement3);
            if (newUser.IsFullyFetched && updateResult.IsFullyFetched)
            {
                return "success";
            }
            else
            {
                return "Writing error.";
            }
        }



        [System.Web.Mvc.HttpGet]
        public ActionResult ViewUser(string user_name)
        {

            if (user_name == null)
            {
                return View("NotFound");
            }

            ISession session = SessionManager.GetSession();

            if (session == null)
                return null;

            var st = session.Prepare("select * from user where user_name = ? ALLOW FILTERING");
            var statement = st.Bind(user_name);

            Row u = session.Execute(statement).FirstOrDefault();
            if(u == null)
            {
                return View("NotFound");
            }

            var st2 = session.Prepare("select * from user_stats where user_name = ?");
            var statement2 = st2.Bind(user_name);

            Row user_stats = session.Execute(statement2).FirstOrDefault();
            if (user_stats == null)
            {
                return View("NotFound");
            }

            User user = new User()
            {
                full_name = u["full_name"].ToString(),
                image_blob = (byte[])u["user_image_blob"],
                user_name = u["user_name"].ToString(),
                profile_description = u["profile_description"].ToString(),
                number_of_followers = (long)user_stats["number_of_followers"],
                number_of_following = (long)user_stats["number_of_following"],
                number_of_posts = (long)user_stats["number_of_posts"]
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
                Session["full_name"] = u["full_name"];
                Session["user_image_blob"] = ((byte[])u["user_image_blob"]);
                Session.Timeout = 60 * 60;
                return "success";
            }
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return View("../Home/Index");
        }

        [System.Web.Mvc.HttpPost]
        public string FollowUser()
        {
            ISession session = SessionManager.GetSession();

            if (Session["user_name"] == null)
            {
                return "you muste be logged in";
            }

            if (session == null)
                return "DB error";

            byte[] following_image_blob = null;
            if (Request["following_image_blob"] != null)
                following_image_blob = Convert.FromBase64String(Request["following_image_blob"]);
            //insert into the people i follow
            var st = session.Prepare("INSERT INTO people_i_follow(user_name, following_user_name, following_full_name, following_image_blob)" +
                                    "VALUES(?, ?, ?, ?)");
            var statement = st.Bind(Session["user_name"], Request["following_user_name"], Request["following_full_name"], following_image_blob);
            //update number of people i follow
            var st2 = session.Prepare("UPDATE user_stats SET number_of_following = number_of_following + 1 WHERE user_name = ?");
            var statement2 = st2.Bind(Session["user_name"]);

            //insert me into that persons followers
            var st3 = session.Prepare("INSERT INTO my_followers(user_name, follower_user_name, follower_full_name, follower_image_blob)" +
                                        "VALUES(?, ?, ?, ?)");
            var statement3 = st3.Bind(Request["following_user_name"], Session["user_name"], Session["full_name"], Session["user_image_blob"]);
            //update that persons number of followers
            var st4 = session.Prepare("UPDATE user_stats SET number_of_followers = number_of_followers + 1 WHERE user_name = ?");
            var statement4 = st4.Bind(Request["following_user_name"]);

            var batch = new BatchStatement()
                .Add(statement)
                .Add(statement3);

            RowSet result = session.Execute(batch);
            RowSet updateStas = session.Execute(statement2);
            RowSet updateStas2 = session.Execute(statement4);
            if (result.IsFullyFetched && updateStas.IsFullyFetched && updateStas2.IsFullyFetched)
            {
                return "success";
            }
            else
            {
                return "error";
            }
        }

        [System.Web.Mvc.HttpPost]
        public string UnfollowUser()
        {
            ISession session = SessionManager.GetSession();

            if (Session["user_name"] == null)
            {
                return "you muste be logged in";
            }

            if (session == null)
                return "DB error";

            //delete from people i follow
            var st = session.Prepare("DELETE FROM people_i_follow WHERE user_name = ? AND following_user_name = ?");
            var statement = st.Bind(Session["user_name"], Request["following_user_name"]);
            //update number of people i follow
            var st2 = session.Prepare("UPDATE user_stats SET number_of_following = number_of_following - 1 WHERE user_name = ?");
            var statement2 = st2.Bind(Session["user_name"]);

            //deletee myself from that persons folllowers
            var st3 = session.Prepare("DELETE FROM my_followers WHERE user_name = ? AND follower_user_name = ?");
            var statement3 = st3.Bind(Request["following_user_name"], Session["user_name"]);
            //update that persons number of followers
            var st4 = session.Prepare("UPDATE user_stats SET number_of_followers = number_of_followers - 1 WHERE user_name = ?");
            var statement4 = st4.Bind(Request["following_user_name"]);

            var batch = new BatchStatement()
                .Add(statement)
                .Add(statement3);

            RowSet result = session.Execute(batch);
            RowSet updateStas = session.Execute(statement2);
            RowSet updateStas2 = session.Execute(statement4);
            if (result.IsFullyFetched && updateStas.IsFullyFetched && updateStas2.IsFullyFetched)
            {
                return "success";
            }
            else
            {
                return "error";
            }
        }


        public ViewResult UserFollowers(string user_name)
        {
            ISession session = SessionManager.GetSession();

            if (session == null)
                return View("Error");

            var st = session.Prepare("select * from my_followers where user_name = ?");
            var statement = st.Bind(user_name);

            RowSet result = session.Execute(statement);
            List<Follower> followers = new List<Follower>();
            foreach (Row row in result)
            {
                followers.Add(new Follower() {
                    follower_full_name = row["follower_full_name"].ToString(),
                    follower_image_blob = (byte[])row["follower_image_blob"],
                    follower_user_name = row["follower_user_name"].ToString()
                });
            }

            if (result != null)
                return View((object)followers);
            else
                return View("Error");
        }

        
        public ViewResult UserFollowing(string user_name)
        {
            ISession session = SessionManager.GetSession();

            if (session == null)
                return View("Error");

            var st = session.Prepare("select * from people_i_follow where user_name = ?");
            var statement = st.Bind(user_name);

            RowSet result = session.Execute(statement);
            List<Follower> followers = new List<Follower>();
            foreach (Row row in result)
            {
                followers.Add(new Follower()
                {
                    follower_full_name = row["following_full_name"].ToString(),
                    follower_image_blob = (byte[])row["following_image_blob"],
                    follower_user_name = row["following_user_name"].ToString()
                });
            }

            if (result != null)
                return View("UserFollowers", (object)followers);
            else
                return View("Error");
        }

        
    }
}