using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Cassandra;

namespace Focus_me.Entities
{
    public class PostSmall
    {
        public System.Guid post_id { get; set; }
        public string user_name { get; set; }
        public string post_image { get; set; }

        public PostSmall(Row row)
        {
            post_id = (System.Guid)row["post_id"];
            user_name = row["user_name"].ToString();
            post_image = "data:image/jpeg;base64," + Convert.ToBase64String((byte[])row["post_blob_thumbnail"]);
        }
    }
}