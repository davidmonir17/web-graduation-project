using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MeetingMinutesGP.Models
{
    public class Meetings
    {
        public string CurrentUserEmail { get; set; }
        public List<Meeting> meetings { get; set; }
        public List<Agendum> agendas { get; set; }
        public List<User_Meeting> users { get; set; }

        public List<Location> loc { get; set; }
        public List<Topic> topics { get; set; }

        public PersonalTask newPersonalTask { get; set; }
        public List<PersonalTask> AllPersonalTasks { get; set; }
        public List<UserMeeting> AllMeetingTasks { get; set; }


    }
}