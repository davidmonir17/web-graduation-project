using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MeetingMinutesGP.Models;
using System.IO;
using System.Web.Mvc;

namespace MeetingMinutesGP.Controllers
{
    public class MeetingNowController : Controller
    {
        GPEntities db = new GPEntities();
        public ActionResult MeetingInformation()
        {
            Location Loc = new Location();
            return View(Loc);
        }
        public ActionResult Participants(Location Loc)
        {
            db.Locations.Add(Loc);
            db.SaveChanges();
            int LastLocID = Loc.LocationID;
            Meeting meeting = new Meeting();
            meeting.locationID = LastLocID;
            meeting.MeetingDate = DateTime.Now;
            meeting.MeetingDuration = null;
            meeting.Status = "Now";
            meeting.MeetingDuration = 5;
            db.Meetings.Add(meeting);
            db.SaveChanges();
            Session["LastMeetingID"] = meeting.MeetingID;
            return View();
        }
        public ActionResult AddParticipants(FormCollection fc)
        {
            string[] DParticipantsEmails = fc["TxtForEmail"].Split(',');
            string[] ParticipantsEmailsWithCurrentUser = DParticipantsEmails.Distinct().ToArray();
            List<string> ParticipantsEmails = ParticipantsEmailsWithCurrentUser.ToList();
            ParticipantsEmails.Remove(System.Web.HttpContext.Current.User.Identity.Name.ToString());
            int meetingID = int.Parse(Session["LastMeetingID"].ToString());
            List<User> users_list = db.Users.ToList();
            for (int i = 0; i < ParticipantsEmails.Count; i++)
            {
                for (int j = 0; j < users_list.Count; j++)
                {
                    if (users_list[j].Email == ParticipantsEmails[i])
                    {
                        int user_id = users_list[j].UserID;
                        UserMeeting user_meeting0 = new UserMeeting();
                        user_meeting0.userID = user_id;
                        user_meeting0.meetingID = meetingID;
                        user_meeting0.Attended = false;
                        user_meeting0.Coordinator = false;
                        db.UserMeetings.Add(user_meeting0);
                        db.SaveChanges();
                        break;
                    }
                }
            }
            string UserEmail = System.Web.HttpContext.Current.User.Identity.Name;
            User CurrentUser = db.Users.Where(a => a.Email == UserEmail).SingleOrDefault();
            UserMeeting user_meeting = new UserMeeting();
            user_meeting.userID = CurrentUser.UserID;
            user_meeting.meetingID = meetingID;
            user_meeting.Attended = true;
            user_meeting.Coordinator = true;
            db.UserMeetings.Add(user_meeting);
            db.SaveChanges();
            return RedirectToAction("NewMeeting");
        }
        public ActionResult NewMeeting()
        {
            int meetingID = int.Parse(Session["LastMeetingID"].ToString());
            MeetingParticipants P = new MeetingParticipants();
            return View(P.GetMeetingParticipants(meetingID));
        }
        [HttpPost]
        public ActionResult CreateMeeting(FormCollection fc)
        {
            string[] agendaTitles = fc["txtAgendaTitle"].Split(',');
            int meetingID = int.Parse(Session["LastMeetingID"].ToString());
            Agendum agenda = new Agendum();
            agenda.meetingID = meetingID;
            agenda.Title = agendaTitles[0];
            db.Agenda.Add(agenda);
            db.SaveChanges();
            Session["AgendaID"] = agenda.AgendaID;
            List<Agendum> agendaList = db.Agenda.ToList();
            int LastAgendaID = agendaList[agendaList.Count - 1].AgendaID;
            string[] TopicsNames = fc["TxtForTpic"].Split(',');
            string[] TopicsDescr = fc["TxtForTpicDescription"].Split(',');
            string[] SubpoicsForTopics = fc["TxtForTpicSubToics"].Split(',');
            List<Topic> topicsList = new List<Topic>();
            var filesCount = Request.Files.Count;
            for (int i = 0; i < TopicsNames.Length; i++)
            {
                Topic topic = new Topic();
                topic.TopicName = TopicsNames[i];
                topic.TopicDescription = TopicsDescr[i];
                topic.agendaId = LastAgendaID;
                db.Topics.Add(topic);
                topicsList.Add(topic);
                db.SaveChanges();
                if (filesCount > 0 && Request.Files[i].ContentLength > 0)
                {
                    var file = Request.Files[i];
                    var fileName = Path.GetFileName(file.FileName);
                    var folderName = topic.TopicID.ToString();
                    var pathString = System.IO.Path.Combine(Server.MapPath("~/App_Data/uploads"), folderName);
                    System.IO.Directory.CreateDirectory(pathString);
                    string path = Path.Combine(pathString, fileName);
                    file.SaveAs(path);
                    topic.FileLocation = path;
                    filesCount--;
                }
                string sub_topics_for_specific_topic= SubpoicsForTopics[i].Replace('-', ',');
                topic.ListOfItems = sub_topics_for_specific_topic;
                db.SaveChanges();
            }
            string[] LinkNames = fc["txtLinkName"].Split(',');
            string[] Links = fc["txtLink"].Split(',');
            Meeting CurrentMeeting = db.Meetings.Where(a => a.MeetingID == meetingID).SingleOrDefault();
            CurrentMeeting.ReferenceLinkName = LinkNames[0];
            CurrentMeeting.ReferenceLink = Links[0];
            db.SaveChanges();

           


            Session["TopicsList"] = topicsList;
            return RedirectToAction("TopicsForVote", "vote");
        }
    }
}