using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MeetingMinutesGP.Models
{
    public class MeetingParticipants
    {
        //public List<User> UsersList = new List<User>();
        //public List<UserMeeting> UserMeetingList = new List<UserMeeting>();
        public int userID;
        public int meetingID;
        public bool attended;
        public string AssignedTask;
        public string TaskStartDate;
        public string TaskEndDate;
        public List<UserMeeting> GetMeetingParticipants(int meetingID)
        {
            GPEntities db = new GPEntities();
            var UserMeetingL = from m in db.UserMeetings
                               where m.meetingID == meetingID
                               select m;
            List<UserMeeting> UserMeetingList = UserMeetingL.ToList();
            return UserMeetingList;
        }
    }
}