using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MeetingMinutesGP.Models
{
    public class ExportModel
    {
        public User leader { get; set; }
        public Meeting meeting { get; set; }
        public List<UserMeeting> user_meeting { get; set; }
        public Location location { get; set; }
        public Agendum agenda { get; set; }
        public List<Topic> topics { get; set; }
        public List<Vote> votes { get; set; }
    }
}