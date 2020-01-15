using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Cassandra;

namespace Focus_me.Entities
{
    public class My_followers
    {
        public string user_name { get; set; }
        public string follower_user_name { get; set; }
        public byte[] follower_image_blob { get; set; }
        public string follower_full_name { get; set; }

        public My_followers(Row row)
        {
            user_name = row["user_name"].ToString();
            follower_full_name = row["follower_full_name"].ToString();
            follower_image_blob = (byte[])row["follower_image_blob"];
            follower_user_name = row["follower_user_name"].ToString();
        }
    }
}