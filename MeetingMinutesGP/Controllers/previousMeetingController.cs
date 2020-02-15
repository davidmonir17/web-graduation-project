using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using MeetingMinutesGP.Models;
namespace MeetingMinutesGP.Controllers
{
    public class previousMeetingController : Controller
    {
        // GET: previousMeeting
        GPEntities ent;
        public ActionResult Index()
        {
            ent = new GPEntities();
            Meetings meetings = new Meetings();
            meetings.meetings = new List<Meeting>();
            meetings.agendas = new List<Agendum>();
            string UserEmail = System.Web.HttpContext.Current.User.Identity.Name;
            var Current_User = ent.Users.Where(a => a.Email == UserEmail).FirstOrDefault();
            int UserID = Current_User.UserID;
            var Current_UserMeeting = ent.UserMeetings.Where(a => a.userID == UserID).ToList();

            for (int j = 0; j < Current_UserMeeting.Count; j++)
            {
                int x = Current_UserMeeting[j].meetingID;
                var Current_Meeting = ent.Meetings.Where(a => a.MeetingID == x).FirstOrDefault();
                if (Current_Meeting.Status == "Closed")
                {
                    meetings.meetings.Add(Current_Meeting);
                    var Current_Agenda = ent.Agenda.Where(a => a.meetingID == Current_Meeting.MeetingID).FirstOrDefault();
                    meetings.agendas.Add(Current_Agenda);
                }
            }
            return View(meetings);
        }
    }
}