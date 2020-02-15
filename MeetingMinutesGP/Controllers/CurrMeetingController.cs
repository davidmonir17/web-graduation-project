using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MeetingMinutesGP.Models;

namespace MeetingMinutesGP.Controllers
{
    public class TreeItem<T>
    {
        public T Item { get; set; }
        public IEnumerable<TreeItem<T>> Children { get; set; }

    }
    internal static class GenericHelpers
    {
        /// <summary>
        /// Generates tree of items from item list
        /// </summary>
        /// 
        /// <typeparam name="T">Type of item in collection</typeparam>
        /// <typeparam name="K">Type of parent_id</typeparam>
        /// 
        /// <param name="collection">Collection of items</param>
        /// <param name="id_selector">Function extracting item's id</param>
        /// <param name="parent_id_selector">Function extracting item's parent_id</param>
        /// <param name="root_id">Root element id</param>
        /// 
        /// <returns>Tree of items</returns>
        public static IEnumerable<TreeItem<T>> GenerateTree<T, K>(
            this IEnumerable<T> collection,
            Func<T, K> id_selector,
            Func<T, K> parent_id_selector,
            K root_id = default(K))
        {
            foreach (var c in collection.Where(c => parent_id_selector(c).Equals(root_id)))
            {
                yield return new TreeItem<T>
                {
                    Item = c,
                    Children = collection.GenerateTree(id_selector, parent_id_selector, id_selector(c))
                };
            }
        }
    }

    public class CurrMeetingController : Controller
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
        // GET: CurrMeeting
        [Authorize]
        public ActionResult viewMeetingContent(int id)
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
            



            MeetingMinutes meetin = new MeetingMinutes();
            meetin.meeting = ent.Meetings.Where(m => m.MeetingID == v.MeetingId).FirstOrDefault();
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
            var root  = toplist.GenerateTree(c => c.TopicID, c => c.subTopicId);
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
            return View(meetin);
        }
        public ActionResult chat()

        {
            uses u1 = new uses();
            string UserEmail = System.Web.HttpContext.Current.User.Identity.Name;
            u1.name = UserEmail;
            //return View();
            return View(u1);

        }
        [Authorize]
        public ActionResult startMeeting(int id)
        {
            MeetingParticipants P = new MeetingParticipants();
            List<UserMeeting> MP = P.GetMeetingParticipants(id);
            for(int i = 0; i < MP.Count; i++)
            {
                
                if (MP[i].Coordinator == true)
                {
                    MP[i].Attended = true;
                    MP.Remove(MP[i]);
                }
            }
            GPEntities ent = new GPEntities();
            Meeting CurrMet = ent.Meetings.Where(a => a.MeetingID == id).SingleOrDefault();
            //CurrMet.Status = "Closed";
            ent.SaveChanges();
            var agenda = ent.Agenda.Where(t => t.meetingID == id).FirstOrDefault();

            var toplist = ent.Topics.Where(t => t.agendaId == agenda.AgendaID).ToList();
            topnames.Clear();
            var root = toplist.GenerateTree(c => c.TopicID, c => c.subTopicId);
            Test(root);
            ViewData["meetigtopicslist"] = topnames;
            Session["CurrentMeeting"] = id;
            return View(MP);
        }

        [Authorize]
        public ActionResult SaveAttendance(List<UserMeeting> MP)
        {
            GPEntities db = new GPEntities();
            for (int i = 0; i < MP.Count; i++)
            {
                int user_id = MP[i].userID;
                int meeting_id = MP[i].meetingID;
                bool attended = MP[i].Attended;
                UserMeeting CurrentUserMeeting = db.UserMeetings.Where(a => a.userID == user_id).Where(a => a.meetingID == meeting_id).FirstOrDefault();
                CurrentUserMeeting.Attended = attended;
                db.SaveChanges();
            }
            return RedirectToAction("startMeeting", "CurrMeeting", new { id=int.Parse(Session["CurrentMeeting"].ToString()) });
        }
        [Authorize]
        public ActionResult customizeReport(int id)
        {
            GPEntities db = new GPEntities();
            ExportModel exportModel = new ExportModel();
            exportModel.meeting = db.Meetings.Where(a => a.MeetingID == id).FirstOrDefault();
            exportModel.user_meeting = db.UserMeetings.Where(a => a.meetingID == id).ToList();
            var leader = db.UserMeetings.Where(a => a.meetingID == id).Where(a=> a.Coordinator==true).FirstOrDefault();
            exportModel.leader = db.Users.Where(a => a.UserID == leader.userID).FirstOrDefault();
            exportModel.location = db.Locations.Where(a => a.LocationID == exportModel.meeting.locationID).FirstOrDefault();
            exportModel.agenda = db.Agenda.Where(a => a.meetingID == id).FirstOrDefault();
            //exportModel.topics = db.Topics.Where(a => a.agendaId == exportModel.agenda.AgendaID).ToList();
            var toplist = db.Topics.Where(t => t.agendaId == exportModel.agenda.AgendaID).ToList();
            topnames.Clear();
            var root = toplist.GenerateTree(c => c.TopicID, c => c.subTopicId);
            Test(root);
            //List<string> rrr = topnames;
            exportModel.topics = topnames;

            List<Vote> votes = new List<Vote>();
            for(int i = 0;i< exportModel.topics.Count; i++)
            {
                int xyz = exportModel.topics[i].TopicID;
                var topicvotes = db.Votes.Where(a => a.topicID == xyz).ToList();
                if (topicvotes != null)
                {
                    for (int j = 0; j < topicvotes.Count; j++)
                    {
                        votes.Add(topicvotes[j]);
                    }
                }
            }
            exportModel.votes = votes;
            GPEntities ent = new GPEntities();
            Meeting CurrMet = ent.Meetings.Where(a => a.MeetingID == id).SingleOrDefault();
            CurrMet.Status = "Closed";
            ent.SaveChanges();
            return View(exportModel);
        }
        public ActionResult ExportRotitva(int id)
        {
            GPEntities db = new GPEntities();
            ExportModel exportModel = new ExportModel();
            exportModel.meeting = db.Meetings.Where(a => a.MeetingID == id).FirstOrDefault();
            exportModel.user_meeting = db.UserMeetings.Where(a => a.meetingID == id).ToList();
            var leader = db.UserMeetings.Where(a => a.meetingID == id).Where(a => a.Coordinator == true).FirstOrDefault();
            exportModel.leader = db.Users.Where(a => a.UserID == leader.userID).FirstOrDefault();
            exportModel.location = db.Locations.Where(a => a.LocationID == exportModel.meeting.locationID).FirstOrDefault();
            exportModel.agenda = db.Agenda.Where(a => a.meetingID == id).FirstOrDefault();
            //exportModel.topics = db.Topics.Where(a => a.agendaId == exportModel.agenda.AgendaID).ToList();
            var toplist = db.Topics.Where(t => t.agendaId == exportModel.agenda.AgendaID).ToList();
            topnames.Clear();
            var root = toplist.GenerateTree(c => c.TopicID, c => c.subTopicId);
            Test(root);
            //List<string> rrr = topnames;
            exportModel.topics = topnames;

            List<Vote> votes = new List<Vote>();
            for (int i = 0; i < exportModel.topics.Count; i++)
            {
                int xyz = exportModel.topics[i].TopicID;
                var topicvotes = db.Votes.Where(a => a.topicID == xyz).ToList();
                if (topicvotes != null)
                {
                    for (int j = 0; j < topicvotes.Count; j++)
                    {
                        votes.Add(topicvotes[j]);
                    }
                }
            }
            exportModel.votes = votes;

            return View(exportModel);
        }
        public ActionResult Export(int id)
        {
            GPEntities db = new GPEntities();
           var agnda = db.Agenda.Where(a => a.meetingID == id).FirstOrDefault();
            //khaled
            return new Rotativa.ActionAsPdf("ExportRotitva", new { id = id })
            {
                //FileName = agnda.Title + ".pdf",
                //PageOrientation = Rotativa.Options.Orientation.Landscape,
                //PageSize = Rotativa.Options.Size.A4
                //PageOrientation = Rotativa.Options.Orientation.Portrait

            };
        }
    }
}