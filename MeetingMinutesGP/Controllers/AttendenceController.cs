using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MeetingMinutesGP.Models;
using System.Web.Mvc;

namespace MeetingMinutesGP.Controllers
{
    public class AttendenceController : Controller
    {
        GPEntities db = new GPEntities();
        [Authorize]
        [HttpPost]
        public ActionResult SaveChanges(List<UserMeeting> MP)
        {
            for (int i = 0; i < MP.Count; i++)
            {
                int user_id = MP[i].userID;
                int meeting_id = MP[i].meetingID;
                bool attended = MP[i].Attended;
                UserMeeting CurrentUserMeeting = db.UserMeetings.Where(a => a.userID == user_id).Where(a => a.meetingID == meeting_id).FirstOrDefault();
                CurrentUserMeeting.Attended = attended;
                db.SaveChanges();
            }
            if (MP.Count != 0)
            {
                Session["meetingID"] = MP[0].meetingID;
                Session["NumberOfUsers"] = MP.Count;
            }
            return RedirectToAction("NewMeeting", "MeetingNow");
        }
        [Authorize]
        public ActionResult AssignTask()
        {
            int meetingID = int.Parse(Session["meetingID"].ToString()),
                NumUsers = int.Parse(Session["NumberOfUsers"].ToString());
            if (NumUsers != 0)
            {
                MeetingParticipants P = new MeetingParticipants();
                List<UserMeeting> MeetingParticipants = P.GetMeetingParticipants(meetingID);
                List<UserMeeting> MeetingParticipantsToView = new List<UserMeeting>();
                for (int i = 0; i < MeetingParticipants.Count; i++)
                {
                    if (MeetingParticipants[i].Attended == true)
                        MeetingParticipantsToView.Add(MeetingParticipants[i]);
                }
                return View(MeetingParticipantsToView);
            }
            return Content("There is no user to assign task to him/her");
        }
        [Authorize]
        public ActionResult SaveTasks(List<UserMeeting>MP)
        {
            int x = 5;
            
                for (int i = 0; i < MP.Count; i++)
                {
                    int user_id = MP[i].userID,
                        meeting_id = MP[i].meetingID;
                    string AssignedTask = MP[i].AssignedTask,
                           TaskDesc = MP[i].TaskDescription,
                           TaskStatus = MP[i].TaskStatus;
                //DateTime TaskStartDate;
                //DateTime TaskEndDate;
                UserMeeting CurrentUserMeeting = db.UserMeetings.Where(a => a.userID == user_id).Where(a => a.meetingID == meeting_id).FirstOrDefault();

                if (MP[i].TaskStartDate != null)
                {
                    CurrentUserMeeting.TaskStartDate = (DateTime)MP[i].TaskStartDate;

                    //TaskStartDate = (DateTime)MP[i].TaskStartDate;

                }
                if (MP[i].TaskEndDate != null)
                {
                    CurrentUserMeeting.TaskEndDate = (DateTime)MP[i].TaskEndDate;

                    //TaskEndDate = (DateTime)MP[i].TaskEndDate;
                }
                string UserEmail = System.Web.HttpContext.Current.User.Identity.Name;
                    CurrentUserMeeting.AssignedTask = AssignedTask;
                    CurrentUserMeeting.TaskDescription = TaskDesc;
                    CurrentUserMeeting.TaskStatus = TaskStatus;
                    CurrentUserMeeting.TaskIntiatorEmail = UserEmail;
                    db.SaveChanges();
                }
            return RedirectToAction("customizeReport", "CurrMeeting", new { id = MP[0].meetingID });

        }

    }
}