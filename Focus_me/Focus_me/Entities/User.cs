using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cassandra;
namespace Focus_me.Entities
{
    public class User
    {
        public string user_name { get; set; }
        public byte[] image_blob { get; set; }
        public string full_name { get; set; }
        public string profile_description { get; set; }
        public string user_password { get; set; }
        public long number_of_followers { get; set; }
        public long number_of_following { get; set; }
        public long number_of_posts { get; set; }

        public bool amIaFollower(string myUsername, string theirUsername)
        {
            ISession session = SessionManager.GetSession();

            if (session == null)
                return false;

            var st = session.Prepare("select * from people_i_follow where user_name = ? and following_user_name = ? ");
            var statement = st.Bind( myUsername, theirUsername);

            Row u = session.Execute(statement).FirstOrDefault();
            if (u == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

}