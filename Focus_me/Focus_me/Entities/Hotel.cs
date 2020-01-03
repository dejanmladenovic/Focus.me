using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Focus_me.Entities
{
    public class Hotel
    {
        public string hotelID { get; set; }
        public string name { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip { get; set; }
    }
}