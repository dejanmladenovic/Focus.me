using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Http;
using Focus_me.Entities;
using Cassandra;

namespace Focus_me.Controllers
{
    public class PostController : Controller
    {
        // GET: Post
        public ActionResult Index()
        {
            return View();
        }

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

            if (Session["user_name"] == null)
            {
                return "you muste be logged in";
            }

            if(post_image == null)
            {
                return "you must upload an image";
            }

            byte[] post_image_blob = null;
            
            if (post_image.ContentType != "image/jpeg")
            {
                return "file format error";
            }
            post_image_blob = new byte[post_image.ContentLength];
            post_image.InputStream.Read(post_image_blob, 0, post_image.ContentLength);

            
            if(post.post_description == null)
            {
                post.post_description = "";
            }
            var st = session.Prepare("insert into \"post\" (\"post_id\",date_posted, user_image_blob, number_of_likes, post_blob, post_description, user_id, user_name)" +
                                            "values (uuid(), ?, ?, 0, ?, ?, ?, ?)");
            
            var statement = st.Bind(DateTime.Now, (byte[])Session["user_image_blob"], post_image_blob, post.post_description, Session["user_id"], Session["user_name"].ToString());

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
    }
}