using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MeetingMinutesGP.Models
{
    public class MeetingMinutes
    {
        public string UserEmail { get; set; }
        public Meeting meeting { get; set; }
        public Location location { get; set; }
        public Agendum agenda { get; set; }
        public List<Topic> topics { get; set; }
        public List<User> users { get; set; }
        public List<string> company { get; set; }
        public List<string> addresses { get; set; }
        public List<User> UserMeeting { get; set; }
        public UserMeeting user_meeting { get; set; }



    }
}