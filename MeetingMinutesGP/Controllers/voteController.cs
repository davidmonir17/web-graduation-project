using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MeetingMinutesGP.Models;
using System.Web.Mvc;

namespace MeetingMinutesGP.Controllers
{
    public class voteController : Controller
    {
        // GET: vote
        //id is agenda id
        GPEntities db = new GPEntities();
        public ActionResult TopicsForVote()
        {
            List<TopicsAttended> topicAttednded = new List<TopicsAttended>();
            List<Topic> TopicsList = (List<Topic>)Session["TopicsList"];
            ViewBag.TopicsList = TopicsList;
            int meetingID = int.Parse(Session["meetingID"].ToString()),
            NumUsers = int.Parse(Session["NumberOfUsers"].ToString());
            List<UserMeeting> MeetingParticipantsToView = new List<UserMeeting>();
            if (NumUsers != 0)
            {
                MeetingParticipants P = new MeetingParticipants();
                List<UserMeeting> MeetingParticipants = P.GetMeetingParticipants(meetingID);
                for (int i = 0; i < MeetingParticipants.Count; i++)
                {
                    if (MeetingParticipants[i].Attended == true)
                        MeetingParticipantsToView.Add(MeetingParticipants[i]);
                }
            }
            return View(MeetingParticipantsToView);
        }
        public ActionResult voteInfo(string id)
        {
            byte[] encoded = Convert.FromBase64String(id);
            int DecodedidTopicID = int.Parse(System.Text.Encoding.UTF8.GetString(encoded));
            Session["TID"] = DecodedidTopicID;
            return RedirectToAction("takeoptions");
        }
        [HttpPost]
        public ActionResult CreateVote(Vote vote)
        {
            Session["CurrentVote"] = vote;
            return RedirectToAction("takeoptions");
        }
        public ActionResult takeoptions()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CreateOptions(FormCollection fc)
        {
            int topicID = int.Parse(Session["TID"].ToString());
            string[] voteName = fc["VoteNameText"].Split(',');
            string[] voteDesc = fc["VoteNameDes"].Split(',');
            Vote currentVote = new Vote();
            currentVote.VoteName = voteName[0];
            currentVote.VoteDescription = voteDesc[0];
            currentVote.topicID = topicID;
            db.Votes.Add(currentVote);
            string[] options = fc["TxtForDescription"].Split(',');
            db.SaveChanges();
            var VotesList = db.Votes.ToList();
            int CurrentVoteID = VotesList[VotesList.Count - 1].VoteID;
            for (int i = 0; i < options.Length; i++)
            {
                Option option = new Option();
                option.voteID = CurrentVoteID;
                option.OptionDescription = options[i];
                option.OptionCount = 0;
                db.Options.Add(option);
                db.SaveChanges();
            }
            var topic = db.Topics.Where(a => a.TopicID == topicID).FirstOrDefault();
            var agena = db.Agenda.Where(a => a.AgendaID == topic.agendaId).FirstOrDefault();


            var usermeet = db.UserMeetings.Where(a => a.meetingID == agena.meetingID).ToList();
            for(int i=0;i< usermeet.Count; i++)
            {
                if (usermeet[i].Attended)
                {
                    Notification notifi = new Notification();
                    notifi.Date = DateTime.Now;
                    notifi.IsRead = false;
                    notifi.UserId = usermeet[i].userID;
                    notifi.MeetingId = usermeet[i].meetingID;
                    notifi.Message = "You are invited to vote on ( " + topic.TopicName + " )";
                    notifi.DetailsURL = "vote/PerformVote";
                    notifi.VoteID = currentVote.VoteID;
                    db.Notifications.Add(notifi);
                    db.SaveChanges();
                }
                
            }

            return RedirectToAction("startMeeting", "CurrMeeting",new { id= agena.meetingID });
            //return RedirectToAction("AssignTask", "Attendence");
        }
        public ActionResult PerformVote(int id)
        {
            GPEntities ent = new GPEntities();

            var v = ent.Notifications.Where(a => a.NotificaionId == id).FirstOrDefault();
            if (v != null)
            {
                v.IsRead = true;

            }
            ent.Entry(v).State = System.Data.Entity.EntityState.Modified;
            ent.Configuration.ValidateOnSaveEnabled = false;
            ent.SaveChanges();


            ViewData["Vote"] = db.Votes.Where(a => a.VoteID == v.VoteID).SingleOrDefault();
            ViewData["optionsList"] = db.Options.Where(a => a.voteID == v.VoteID).ToList();
            Option UserOption = new Option();
            return View(UserOption);
        }
        public void UserVote(Option option)
        {
            Option UserOption = db.Options.Where(a => a.OptionID == option.OptionID).SingleOrDefault();
            UserOption.OptionCount += 1;
            db.SaveChanges();


            string UserEmail = System.Web.HttpContext.Current.User.Identity.Name;
            var Current_user = db.Users.Where(a => a.Email == UserEmail).FirstOrDefault();

            var voteelem = db.Votes.Where(a => a.VoteID == UserOption.voteID).FirstOrDefault();
            //var topicelem = db.Topics.Where(a => a.TopicID == voteelem.topicID).FirstOrDefault();
            //var agena = db.Agenda.Where(a => a.AgendaID == topicelem.agendaId).FirstOrDefault();
            //var meeting = db.Agenda.Where(a => a.meetingID == agena.meetingID).FirstOrDefault();

            Notification Notifi = new Notification();
            Notifi = db.Notifications.Where(a => a.UserId == Current_user.UserID).Where(a=>a.VoteID == voteelem.VoteID).FirstOrDefault();
            db.Notifications.Remove(Notifi);
            db.SaveChanges();
            //return;
        }


    }
}