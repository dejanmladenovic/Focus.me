using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Focus_me.Entities
{
    public class User
    {
        public string id { get; set; }
        public string user_name { get; set; }
        public byte[] image_blob { get; set; }
        public string full_name { get; set; }
        public string profile_description { get; set; }
        public string user_password { get; set; }
        public int number_of_followers { get; set; }
        public int number_of_following { get; set; }
        public int number_of_posts { get; set; }
    }
}