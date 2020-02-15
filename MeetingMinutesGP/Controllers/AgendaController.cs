using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MeetingMinutesGP.Models;

namespace MeetingMinutesGP.Controllers
{
    public class AgendaController : Controller
    {
        public static List<Topic> topnames = new List<Topic>();

        static void Test(IEnumerable<TreeItem<Topic>> categories, int deep = 0)
        {
            foreach (var c in categories)
            {

                //topnames.Add(new String('\t', deep) + c.Item.TopicName);
                Topic temptopic = new Topic();
                temptopic.TopicID = c.Item.TopicID;
                temptopic.TopicName = (new String('.', deep) + c.Item.TopicName);
                temptopic.TopicDescription = c.Item.TopicDescription;
                temptopic.TopicTime = c.Item.TopicTime;
                temptopic.TopicPriority = c.Item.TopicPriority;
                temptopic.agendaId = c.Item.agendaId;
                temptopic.ListOfItems = c.Item.ListOfItems;
                temptopic.FileLocation = c.Item.FileLocation;
                temptopic.subTopicId = c.Item.subTopicId;
                topnames.Add(temptopic);
                //Console.WriteLine(new String('\t', deep) + c.Item.TopicName);
                Test(c.Children, deep + 1);
            }
            //return topnames;
        }
        // GET: Agenda
        [Authorize]
        public ActionResult Index(string id)
        {

            byte[] encoded = Convert.FromBase64String(id);
            int DecodedId = int.Parse(System.Text.Encoding.UTF8.GetString(encoded));
            GPEntities ent = new GPEntities();
            MeetingMinutes meetin = new MeetingMinutes();
            meetin.meeting = ent.Meetings.Where(m => m.MeetingID == DecodedId).FirstOrDefault();
            meetin.location = ent.Locations.Where(l => l.LocationID == meetin.meeting.locationID).FirstOrDefault();
            List<UserMeeting> user_meetin = new List<UserMeeting>();
            user_meetin = ent.UserMeetings.Where(u => u.meetingID == meetin.meeting.MeetingID).ToList();

            meetin.UserMeeting = new List<User>();
            for (int i = 0; i < user_meetin.Count; i++)
            {
                int x = user_meetin[i].userID;
                var user = ent.Users.Where(u => u.UserID == x).FirstOrDefault();
                if (user_meetin[i].Coordinator == true)
                {
                    meetin.UserEmail = user.Email;
                }
                else
                {
                    meetin.UserMeeting.Add(user);
                }
            }
            meetin.users = new List<User>();
            meetin.users = ent.Users.Where(u => u.Email != meetin.UserEmail).ToList();
            meetin.agenda = ent.Agenda.Where(a => a.meetingID == meetin.meeting.MeetingID).FirstOrDefault();
            meetin.topics = new List<Topic>();
            //meetin.topics = ent.Topics.Where(t => t.agendaId == meetin.agenda.AgendaID).ToList();

            var toplist = ent.Topics.Where(t => t.agendaId == meetin.agenda.AgendaID).ToList();
            topnames.Clear();
            var root = toplist.GenerateTree(c => c.TopicID, c => c.subTopicId);
            Test(root);
            //List<string> rrr = topnames;
            meetin.topics = topnames;
            //location list in combo
            var locLst = ent.Locations.ToList();
            var Current_User = ent.Users.Where(a => a.Email == meetin.UserEmail).FirstOrDefault();
            meetin.company = new List<string>();
            meetin.company.Add(Current_User.Company);
            List<string> locNames = new List<string>();
            for (var i = 0; i < locLst.Count; i++)
            {
                if (locLst[i].Company == meetin.company[0])
                {
                    locNames.Add(locLst[i].Address);
                }
            }
            meetin.addresses = locNames.Distinct().ToList();
            var Current_Meeting = ent.Meetings.Where(a => a.MeetingID == DecodedId).FirstOrDefault();
            if (DateTime.Compare(DateTime.Now, Current_Meeting.MeetingDate.AddHours((double)Current_Meeting.MeetingDuration)) < 0)
            {
                return View(meetin);
            }
            else if ((DateTime.Compare(DateTime.Now, Current_Meeting.MeetingDate) >= 0)&&(DateTime.Compare(DateTime.Now, Current_Meeting.MeetingDate.AddHours((double)Current_Meeting.MeetingDuration)) < 0))
            {
                return RedirectToAction("startMeeting", "CurrMeeting", new { id = Current_Meeting.MeetingID });
            }
            return View(meetin);
        }

        [Authorize]
        public ActionResult Action(int TopicId)
        {
            GPEntities ent = new GPEntities();
            var Current_Topic = ent.Topics.Where(a => a.TopicID == TopicId).FirstOrDefault();
            string fileLoc = Current_Topic.FileLocation;
            string[] name = fileLoc.Split('\\');
            byte[] fileBytes = System.IO.File.ReadAllBytes(fileLoc);
            string fileName = name[name.Count() - 1];
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }
        public ActionResult Download(string path)
        {

            string fileLoc = path;
            string[] name = fileLoc.Split('\\');
            byte[] fileBytes = System.IO.File.ReadAllBytes(fileLoc);
            string fileName = name[name.Count() - 1];
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

    }
}