using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Cassandra;


namespace Focus_me.Entities
{
    public class Post
    {
        public TimeUuid post_id { get; set; }
        public string user_name { get; set; }
        public byte[] user_image_blob { get; set; }
        public byte[] post_blob { get; set; }
        public string post_description { get; set; }
        public long number_of_likes { get; set; }
        public DateTime date_posted { get; set; }
        public bool did_i_like { get; set; }

        public Post()
        {

        }

        public Post(Row row)
        {
            post_id = TimeUuid.Parse(row["post_id"].ToString());
            user_name = row["user_name"].ToString();
            post_description = row["post_description"].ToString();
            post_blob = (byte[])row["post_blob"];
            user_image_blob = (byte[])row["user_image_blob"];
            date_posted = DateTime.Parse(row["date_posted"].ToString()); //(DateTime)row["date_posted"] new DateTime
        }

        public void SetLikeFlag(int rowCount)
        {
            if(rowCount > 0)
            {
                did_i_like = true;
            }
            else
            {
                did_i_like = false;
            }
        }
    }
}