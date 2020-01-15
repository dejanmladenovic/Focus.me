using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Focus_me.Entities
{
    public class Follower
    {
        public string follower_user_name { get; set; }
        public byte[] follower_image_blob { get; set; }
        public string follower_full_name { get; set; }
    }
}