using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Http;
using System.Web.Script.Serialization;
using Focus_me.Entities;
using Cassandra;
using System.Text;

namespace Focus_me.Controllers
{
    public class PostController : Controller
    {

        public ActionResult Upload()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        [System.Web.Mvc.HttpPost]
        public string UploadPost([FromBody]Post post)
        {
            ISession session = SessionManager.GetSession();

            if (session == null)
                return "Session error";

            HttpPostedFileBase post_image = Request.Files["post_image_blob"];
            HttpPostedFileBase post_image_thumbnail = Request.Files["post_image_blob_thumbnail"];

            if (Session["user_name"] == null)
            {
                return "you muste be logged in";
            }

            if(post_image == null || post_image_thumbnail == null)
            {
                return "you must upload an image";
            }

            byte[] post_image_blob = null;
            byte[] post_image_blob_thumbnail = null;
            if (post_image.ContentType != "image/jpeg" || post_image_thumbnail.ContentType != "image/jpeg")
            {
                return "file format error";
            }
            post_image_blob = new byte[post_image.ContentLength];
            post_image.InputStream.Read(post_image_blob, 0, post_image.ContentLength);
            post_image_blob_thumbnail = new byte[post_image_thumbnail.ContentLength];
            post_image_thumbnail.InputStream.Read(post_image_blob_thumbnail, 0, post_image_thumbnail.ContentLength);

            if (post.post_description == null)
            {
                post.post_description = "";
            }

            System.Guid newID = System.Guid.NewGuid();

            var st = session.Prepare("insert into \"post\" (\"post_id\", user_image_blob, post_blob, post_blob_thumbnail, post_description, user_name)" +
                                            "values (now(), ?, ?, ?, ?, ?)");
            var st2 = session.Prepare("UPDATE user_stats SET number_of_posts = number_of_posts + 1 " +
                                        "WHERE user_name = ?; ");

            
            var statement = st.Bind((byte[])Session["user_image_blob"], post_image_blob, post_image_blob_thumbnail, post.post_description, Session["user_name"].ToString());
            var statement2 = st2.Bind(Session["user_name"].ToString());
            var batch = new BatchStatement().Add(statement);

            RowSet newImage = session.Execute(statement);
            RowSet updateStats = session.Execute(statement2);
            if (newImage.IsFullyFetched && updateStats.IsFullyFetched/* && updateStats2.IsFullyFetched*/)
            {
                return "success";
            }
            else
            {
                return "Writing error.";
            }
        }

        public string GetPostsByUser(string user_name, string pagination)
        {
            ISession session = SessionManager.GetSession();

            if (session == null)
                return "Session error";

            var st = session.Prepare("select post_id, user_name, post_blob_thumbnail from post where user_name = ?");
            var statement = st.Bind(user_name).SetAutoPage(false).SetPageSize(3);

            if(pagination != null)
            {
                statement.SetPagingState(Convert.FromBase64String(pagination));
            }

            RowSet result = session.Execute(statement);
            List<PostSmall> posts = new List<PostSmall>();
            foreach(Row row in result)
            {
                posts.Add(new PostSmall(row));
            }

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string pagingState = "end";
            if(result.PagingState != null)
            {
                pagingState = Convert.ToBase64String(result.PagingState);
            }
            
            string returnStr = "{\"posts\": " + serializer.Serialize(posts) + ", \"pagination\": \"" + pagingState + "\"}";

            return returnStr;
        }

        public ViewResult GetSinglePost(string user, string id)
        {
            ISession session = SessionManager.GetSession();

            System.Guid post_id = System.Guid.Parse(id);

            if (session == null)
                return View("Error");

            var st = session.Prepare("SELECT user_name, post_id, toTimestamp(post_id) as date_posted, number_of_likes, post_blob, post_description, user_image_blob " +
                                    "FROM post WHERE user_name= ? AND post_id = ?");
            var statement = st.Bind(user, post_id);
            var st2 = session.Prepare("SELECT post_likes FROM post_stats WHERE post_id = ?");
            var statement2 = st2.Bind(post_id);



            Row result = session.Execute(statement).FirstOrDefault(); ;
            if(result.Count() == 0)
                return View("Error");

            Post post = new Post(result);

            Row result2 = session.Execute(statement2).FirstOrDefault(); ;

            if (result2 != null)
                post.number_of_likes = (long)result2["post_likes"];
            else
                post.number_of_likes = 0;

            if(Session["user_name"] == null)
            {
                post.did_i_like = false;
            }
            else
            {
                var st3 = session.Prepare("SELECT user_name FROM post_likes WHERE post_id = ? AND user_name = ?;");
                var statement3 = st3.Bind(post_id, Session["user_name"].ToString());
                RowSet result3 = session.Execute(statement3);
                post.SetLikeFlag(result3.Count());
            }
            return View((object)post);
        }

        public string LikePost(string post_id)
        {
            ISession session = SessionManager.GetSession();

            if (Session["user_name"] == null)
            {
                return "you muste be logged in";
            }

            if (session == null)
                return "Session error";

            System.Guid id = System.Guid.Parse(post_id);

            var st = session.Prepare("INSERT INTO post_likes(post_id, user_name, full_name, image_blob) VALUES (?, ?, ?, ?)");
            var statement = st.Bind(id, Session["user_name"].ToString(), Session["full_name"].ToString(), Session["user_image_blob"]);

            var st2 = session.Prepare("UPDATE post_stats SET post_likes = post_likes + 1 WHERE post_id = ?");
            var statement2 = st2.Bind(id);

            RowSet result = session.Execute(statement);
            
            if (result.IsFullyFetched)
            {
                RowSet updateStats = session.Execute(statement2);
                if(updateStats.IsFullyFetched)
                {
                    return "success";
                }
                else
                {
                    return "Writing error.";
                }
                
            }
            else
            {
                return "Writing error.";
            }
        }

        public string UnlikePost(string post_id)
        {
            ISession session = SessionManager.GetSession();

            if (Session["user_name"] == null)
            {
                return "you muste be logged in";
            }

            if (session == null)
                return "Session error";

            System.Guid id = System.Guid.Parse(post_id);

            var st = session.Prepare("DELETE FROM post_likes WHERE post_id = ? AND user_name = ?");
            var statement = st.Bind(id, Session["user_name"].ToString());

            var st2 = session.Prepare("UPDATE post_stats SET post_likes = post_likes - 1 WHERE post_id = ?");
            var statement2 = st2.Bind(id);

            RowSet result = session.Execute(statement);
            if (result.IsFullyFetched)
            {
                RowSet updateStats = session.Execute(statement2);
                if (updateStats.IsFullyFetched)
                {
                    return "success";
                }
                else
                {
                    return "Writing error.";
                }
            }
            else
            {
                return "Writing error.";
            }
        }

        public ViewResult GetUsersThatLikedPost(string post_id)
        {
            ISession session = SessionManager.GetSession();

            if (session == null)
                return View("Error");

            var st = session.Prepare("select * from post_likes where post_id = ?");
            var statement = st.Bind(System.Guid.Parse(post_id));

            RowSet result = session.Execute(statement);
            List<Follower> followers = new List<Follower>();
            foreach (Row row in result)
            {
                followers.Add(new Follower()
                {
                    follower_full_name = row["full_name"].ToString(),
                    follower_image_blob = (byte[])row["image_blob"],
                    follower_user_name = row["user_name"].ToString()
                });
            }

            if (result != null)
                return View("~/Views/Users/UserFollowers.cshtml", (object)followers);
            else
                return View("Error");
        }

        [ValidateAntiForgeryToken]
        [System.Web.Mvc.HttpPost]
        public string UpdatePost()
        {
            ISession session = SessionManager.GetSession();

            if (Session["user_name"] == null)
            {
                return "you muste be logged in";
            }

            if (session == null)
                return "Session error";

            var st = session.Prepare("UPDATE post SET post_description = ? WHERE user_name = ? AND post_id = ?");
            var statement = st.Bind(Request["post_description"].ToString(), Session["user_name"], System.Guid.Parse(Request["post_id"].ToString()));

            RowSet result = session.Execute(statement);

            if (result.IsFullyFetched)
            {
                return "success";
            }
            else
            {
                return "Writing error.";
            }
        }

        public string DeletePost(string post_id)
        {
            ISession session = SessionManager.GetSession();

            if (Session["user_name"] == null)
            {
                return "you muste be logged in";
            }

            if (session == null)
                return "Session error";

            var st = session.Prepare("DELETE FROM post  WHERE user_name = ? AND post_id = ?");
            var statement = st.Bind(Session["user_name"], System.Guid.Parse(post_id));

            RowSet result = session.Execute(statement);

            if (result.IsFullyFetched)
            {
                return "success";
            }
            else
            {
                return "Writing error.";
            }
        }

        public ViewResult FollowingPosts()
        {
            ISession session = SessionManager.GetSession();

            if (session == null || Session["user_name"] == null)
                return View("Error");


            var st = session.Prepare("select following_user_name from people_i_follow where user_name = ?");
            var statement = st.Bind(Session["user_name"]);



            RowSet result = session.Execute(statement);
            List<String> names = new List<String>();
            foreach (Row row in result)
            {
                names.Add(row["following_user_name"].ToString());
            }


            var st2 = session.Prepare("SELECT user_name, post_id, toTimestamp(post_id) as date_posted, number_of_likes, post_blob, post_description, user_image_blob " +
                                    "FROM post WHERE user_name in :names");
            var statement2 = st2.Bind(names).SetAutoPage(false).SetPageSize(10);

            if(Session["pagination"] != null)
            {
                statement2.SetPagingState((byte[])Session["pagination"]);
            }

            RowSet result2 = session.Execute(statement2);
            Session["pagination"] = result2.PagingState;

            List<Post> posts = new List<Post>();
            foreach(Row row in result2)
            {
                Post post = new Post(row);
                var st3 = session.Prepare("SELECT post_likes FROM post_stats WHERE post_id = ?");
                var statement3 = st3.Bind(post.post_id);
                var result3 = session.Execute(statement3).FirstOrDefault();

                if (result3 != null)
                    post.number_of_likes = (long)result3["post_likes"];
                else
                    post.number_of_likes = 0;

                var st4 = session.Prepare("SELECT user_name FROM post_likes WHERE post_id = ? AND user_name = ?;");
                var statement4 = st4.Bind(post.post_id, Session["user_name"].ToString());
                RowSet result4 = session.Execute(statement4);
                post.SetLikeFlag(result4.Count());

                posts.Add(post);
            }

            return View((object)posts);
        }

        public ViewResult Index()
        {
            if (Session["user_name"] == null)
                return View("~/Views/Users/Login.cshtml");

            Session["pagination"] = null;

            return View();
        }
    }
}