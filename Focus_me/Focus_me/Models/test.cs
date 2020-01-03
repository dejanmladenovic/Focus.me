using Cassandra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Focus_me.Models
{
    public class test
    {
        public static void AddHotel(string hotelID)
        {
            ISession session = SessionManager.GetSession();

            if (session == null)
                return;

            RowSet hotelData = session.Execute("insert into \"Hotel\" (\"hotelID\", address, city, name, phone, state, zip)  values ('" + hotelID + "', 'Vozda Karadjordja 12', 'Nis', 'Grand', '123', 'Srbija', '18000')");

        }
    }
}