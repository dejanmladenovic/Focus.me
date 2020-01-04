using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Focus_me.Entities
{
    public class Post_likes
    {
        public string post_id { get; set; }
        public string user_id { get; set; }
        public string user_name { get; set; }
        public byte[] image_blob { get; set; }
        public string full_name { get; set; }

    }
}