using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MeetingMinutesGP.Models;
using System.Data.Entity.Migrations;
using System.IO;

namespace MeetingMinutesGP.Controllers
{

    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        [Authorize]
        public ActionResult TasksAndMeetings()
        {
            GPEntities ent = new GPEntities();
            Meetings meetings = new Meetings();
            string UserEmail = System.Web.HttpContext.Current.User.Identity.Name;
            var Current_User = ent.Users.Where(a => a.Email == UserEmail).FirstOrDefault();
            int UserID = Current_User.UserID;
            var Current_UserMeeting = ent.UserMeetings.Where(a => a.userID == UserID).ToList();
            List<int> MeetingID = new List<int>();
            for (int i = 0; i < Current_UserMeeting.Count; i++)
            {
                MeetingID.Add(Current_UserMeeting[i].meetingID);
            }
            List<int> meetingIds = new List<int>();
            meetings.meetings = new List<Meeting>();
            meetings.agendas = new List<Agendum>();
            for (int j = 0; j < MeetingID.Count; j++)
            {
                int x = MeetingID[j];
                var Current_Meeting = ent.Meetings.Where(a => a.MeetingID == x).FirstOrDefault();
                if (Current_Meeting != null && DateTime.Compare(DateTime.Now, Current_Meeting.MeetingDate.AddHours((double)Current_Meeting.MeetingDuration)) <= 0 && Current_Meeting.Status != "Closed")
                {
                    meetingIds.Add(Current_Meeting.MeetingID);
                    meetings.meetings.Add(Current_Meeting);
                    var Current_Agenda = ent.Agenda.Where(a => a.meetingID == Current_Meeting.MeetingID).FirstOrDefault();
                    meetings.agendas.Add(Current_Agenda);
                }
            }
            meetings.users = new List<User_Meeting>();
            for (int k = 0; k < meetingIds.Count; k++)
            {
                User_Meeting u = new User_Meeting();

                int y = meetingIds[k];
                var Current = ent.UserMeetings.Where(a => a.meetingID == y).ToList();
                if (Current != null)
                {
                    u.MeetingID = y;
                    u.Email = new List<string>();
                    for (int j = 0; j < Current.Count; j++)
                    {
                        int x = Current[j].userID;
                        string email = ent.Users.Where(a => a.UserID == x).FirstOrDefault().Email;
                        u.Email.Add(email);
                    }
                    meetings.users.Add(u);
                }

            }
            meetings.loc = new List<Location>();
            for (int i = 0; i < meetings.meetings.Count; i++)
            {
                if (meetings.meetings[i].locationID != null)
                {
                    int z = (int)meetings.meetings[i].locationID;
                    var Current_Loc = ent.Locations.Where(a => a.LocationID == z).FirstOrDefault();
                    if (Current_Loc != null)
                    {
                        meetings.loc.Add(Current_Loc);
                    }
                }

            }


            meetings.CurrentUserEmail = System.Web.HttpContext.Current.User.Identity.Name;

            meetings.AllPersonalTasks = new List<PersonalTask>();
            meetings.AllMeetingTasks = new List<UserMeeting>();


            var Current_personalTasks = ent.PersonalTasks.Where(a => a.userId == UserID).ToList();

            meetings.AllPersonalTasks = Current_personalTasks;


            var currentMeetingTasks = ent.UserMeetings.Where(a => a.userID == UserID).ToList();
            for (var i = 0; i < currentMeetingTasks.Count; i++)
            {
                if (currentMeetingTasks[i].AssignedTask != null)
                {
                    meetings.AllMeetingTasks.Add(currentMeetingTasks[i]);
                }
            }


            return View(meetings);
        }

        public ActionResult EditMeeting(string id)
        {
            byte[] encoded = Convert.FromBase64String(id);
            int Decodedid = int.Parse(System.Text.Encoding.UTF8.GetString(encoded));
            MeetingMinutes meetin = new MeetingMinutes();
            GPEntities ent = new GPEntities();
            meetin.meeting = ent.Meetings.Where(m => m.MeetingID == Decodedid).FirstOrDefault();
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
            meetin.topics = ent.Topics.Where(t => t.agendaId == meetin.agenda.AgendaID).ToList();

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

        [ValidateInput(false)]
        public ActionResult UpdateMeeting(MeetingMinutes meetin)
        {
            GPEntities ent = new GPEntities();

            var meeting = ent.Meetings.Where(m => m.MeetingID == meetin.meeting.MeetingID).SingleOrDefault();

            if (meetin.location.Address != null)
            {
                var locs = ent.Locations.ToList();

                var locID = 0;
                for (var i = 0; i < locs.Count; i++)
                {
                    if (locs[i].Company == meetin.location.Company && locs[i].Address == meetin.location.Address && locs[i].Floor == meetin.location.Floor && locs[i].RoomNumber == meetin.location.RoomNumber)
                    {
                        locID = locs[i].LocationID;
                        meeting.locationID = locID;
                        break;
                    }
                }
                if (locID == 0)
                {
                    Location l = new Location();
                    l = meetin.location;
                    ent.Locations.Add(l);
                    ent.SaveChanges();
                    meeting.locationID = l.LocationID;
                }
            }


            //var meeting =  ent.Meetings.Where(m => m.MeetingID == meetin.meeting.MeetingID).SingleOrDefault();
            meeting.MeetingDate = meetin.meeting.MeetingDate;
            meeting.MeetingDuration = meetin.meeting.MeetingDuration;
            ent.Meetings.AddOrUpdate(meeting);
            ent.SaveChanges();

            var agenda = ent.Agenda.Where(a => a.meetingID == meetin.meeting.MeetingID).SingleOrDefault();
            agenda.Title = meetin.agenda.Title;
            ent.Agenda.AddOrUpdate(agenda);
            ent.SaveChanges();

            var topics = ent.Topics.Where(t => t.agendaId == agenda.AgendaID).ToList();


            ent.Topics.RemoveRange(topics);
            ent.SaveChanges();

            var filesCount = Request.Files.Count;
            for (int i = 0; i < meetin.topics.Count; i++)
            {
                if (meetin.topics[i].TopicName != null)
                {
                    meetin.topics[i].agendaId = agenda.AgendaID;
                    if (meetin.topics[i].subTopicId != null)
                    {
                        meetin.topics[i].subTopicId = meetin.topics[(int)meetin.topics[i].subTopicId].TopicID;
                    }








                    string id = meetin.topics[i].TopicName.ToLower();
                    string[] arrToCheck = new string[] { "ourselves", "hers", "between", "yourself", "but", "again", "there", "about", "once", "during", "out", "very", "having", "with", "they", "own", "an", "be", "some", "for", "do", "its", "yours", "such", "into", "of", "most", "itself", "other", "off", "is", "s", "am", "or", "who", "as", "from", "him", "each", "the", "themselves", "until", "below", "are", "we", "these", "your", "his", "through", "don", "nor", "me", "were", "her", "more", "himself", "this", "down", "should", "our", "their", "while", "above", "both", "up", "to", "ours", "had", "she", "all", "no", "when", "at", "any", "before", "them", "same", "and", "been", "have", "in", "will", "on", "does", "yourselves", "then", "that", "because", "what", "over", "why", "so", "can", "did", "not", "now", "under", "he", "you", "herself", "has", "just", "where", "too", "only", "myself", "which", "those", "i", "after", "few", "whom", "t", "being", "if", "theirs", "my", "against", "a", "by", "doing", "it", "how", "further", "was", "here", "than", "" };
                    string input = (id);
                    string[] arr = input.Split(' ');
                    List<string> AfterRemovingPunc = new List<string>();
                    string output = "";
                    bool flagi = false;
                    foreach (string value in arr)
                    {
                        AfterRemovingPunc.Add(TrimPunctuation(value));
                    }
                    for (int ii = 0; ii < AfterRemovingPunc.Count; ii++)
                    {
                        if (!arrToCheck.Contains(AfterRemovingPunc[ii]))
                        {

                            if (flagi == false)
                            {
                                output = output + AfterRemovingPunc[ii];
                                flagi = true;
                            }
                            else
                            {
                                output = output + " " + AfterRemovingPunc[ii];
                            }
                        }
                    }
                    string[] TopicWords = output.Split(' ');
                    TopicWords = TopicWords.Distinct().ToArray();
                    meetin.topics[i].TopicImpWords = string.Join(" ", TopicWords);

                    











                    ent.Topics.Add(meetin.topics[i]);
                    ent.SaveChanges();

                    var flag = meetin.topics[i].FileLocation;

                    if (filesCount > 0 && Request.Files[i].ContentLength > 0)
                    {
                        var file = Request.Files[i];
                        // extract only the filename
                        var fileName = Path.GetFileName(file.FileName);
                        // store the file inside ~/App_Data/uploads folder
                        var topic = ent.Topics.ToList();
                        var folderName = topic.Last().TopicID.ToString();
                        var pathString = System.IO.Path.Combine(Server.MapPath("~/App_Data/uploads/Topic_Files"), folderName);
                        System.IO.Directory.CreateDirectory(pathString);
                        string path = Path.Combine(pathString, fileName);
                        file.SaveAs(path);
                        meetin.topics[i].FileLocation = path;
                        filesCount--;
                    }

                    else if (flag != null)
                    {
                        var topic = ent.Topics.ToList();
                        var folderName = topic.Last().TopicID.ToString();
                        string[] name = flag.Split('+');
                        name[name.Count() - 2] = folderName;
                        string c = "";
                        for (int j = 0; j < name.Count(); j++)
                        {
                            if (name.Count() - 1 == j)
                            {
                                c += name[j];
                            }
                            else
                            {
                                c += name[j] + "\\";
                            }

                        }

                        meetin.topics[i].FileLocation = c;

                    }


                    ent.SaveChanges();
                }
            }
            var notfications = ent.Notifications.Where(u => u.MeetingId == meeting.MeetingID).ToList();
            ent.Notifications.RemoveRange(notfications);

            var user_meeting = ent.UserMeetings.Where(u => u.meetingID == meeting.MeetingID).ToList();
            ent.UserMeetings.RemoveRange(user_meeting);
            ent.SaveChanges();

            string coordinator_email = System.Web.HttpContext.Current.User.Identity.Name;

            UserMeeting usermeeting = new UserMeeting();
            var coordinator = ent.Users.Where(u => u.Email == coordinator_email).FirstOrDefault();
            usermeeting.meetingID = meeting.MeetingID;
            usermeeting.userID = coordinator.UserID;
            usermeeting.Coordinator = true;
            usermeeting.Attended = false;
            ent.UserMeetings.Add(usermeeting);
            ent.SaveChanges();
            var users = ent.Users.ToList();
            for (int j = 0; j < meetin.UserMeeting.Count; j++)
            {
                for (int i = 0; i < users.Count; i++)
                {

                    string g = meetin.UserMeeting[j].Email;
                    if (users[i].Email == g)
                    {

                        usermeeting = new UserMeeting();
                        usermeeting.meetingID = meeting.MeetingID;
                        usermeeting.userID = users[i].UserID;
                        usermeeting.Attended = false;
                        ent.UserMeetings.Add(usermeeting);


                        Notification notifiCurrent_Users = new Notification();
                        notifiCurrent_Users.Date = meetin.meeting.MeetingDate;
                        notifiCurrent_Users.IsRead = false;
                        notifiCurrent_Users.UserId = users[i].UserID;
                        notifiCurrent_Users.MeetingId = meetin.meeting.MeetingID;
                        notifiCurrent_Users.Message = "Modified meeting ( " + meetin.agenda.Title + " )";
                        notifiCurrent_Users.DetailsURL = "CurrMeeting/viewMeetingContent";
                        //notifiCurrent_User.VoteID = 0;
                        ent.Notifications.Add(notifiCurrent_Users);
                        ent.SaveChanges();
                    }
                }
            }
            Notification notifiCurrent_User = new Notification();
            notifiCurrent_User.Date = meetin.meeting.MeetingDate;
            notifiCurrent_User.IsRead = false;
            notifiCurrent_User.UserId = coordinator.UserID;
            notifiCurrent_User.MeetingId = meetin.meeting.MeetingID;
            notifiCurrent_User.Message = "Modified meeting ( " + meetin.agenda.Title + " )";
            notifiCurrent_User.DetailsURL = "CurrMeeting/viewMeetingContent";
            //notifiCurrent_User.VoteID = 0;
            ent.Notifications.Add(notifiCurrent_User);
            ent.SaveChanges();


            return RedirectToAction("TasksAndMeetings", "Home");
        }
        static string TrimPunctuation(string value)
        {
            // Count start punctuation.
            int removeFromStart = 0;
            for (int i = 0; i < value.Length; i++)
            {
                if (char.IsPunctuation(value[i]))
                {
                    removeFromStart++;
                }
                else
                {
                    break;
                }
            }

            // Count end punctuation.
            int removeFromEnd = 0;
            for (int i = value.Length - 1; i >= 0; i--)
            {
                if (char.IsPunctuation(value[i]))
                {
                    removeFromEnd++;
                }
                else
                {
                    break;
                }
            }
            // No characters were punctuation.
            if (removeFromStart == 0 &&
                removeFromEnd == 0)
            {
                return value;
            }
            // All characters were punctuation.
            if (removeFromStart == value.Length &&
                removeFromEnd == value.Length)
            {
                return "";
            }
            // Substring.
            return value.Substring(removeFromStart,
                value.Length - removeFromEnd - removeFromStart);
        }


        public ActionResult DeleteMeeting(string id)
        {
            byte[] encoded = Convert.FromBase64String(id);
            int DecodedId = int.Parse(System.Text.Encoding.UTF8.GetString(encoded));
            GPEntities ent = new GPEntities();
            var Current_Meeting = ent.Meetings.Where(a => a.MeetingID == DecodedId).FirstOrDefault();
            var Meeting_notification = ent.Notifications.Where(a => a.MeetingId == DecodedId).ToList();
            var Current_Agenda = ent.Agenda.Where(a => a.meetingID == DecodedId).FirstOrDefault();
            var Current_topics = ent.Topics.Where(a => a.agendaId == Current_Agenda.AgendaID).ToList();
            var Current_userMeetings = ent.UserMeetings.Where(a => a.meetingID == DecodedId).ToList();
            if (Meeting_notification !=null)
            {
                ent.Notifications.RemoveRange(Meeting_notification);
            }
            ent.Topics.RemoveRange(Current_topics);
            ent.SaveChanges();
            ent.Agenda.Remove(Current_Agenda);
            ent.SaveChanges();
            ent.UserMeetings.RemoveRange(Current_userMeetings);
            ent.SaveChanges();
            ent.Meetings.Remove(Current_Meeting);
            ent.SaveChanges();
            return RedirectToAction("TasksAndMeetings", "Home");
        }
        public ActionResult AsssignTask(PersonalTask newPersonalTask)
        {
            GPEntities ent = new GPEntities();
            string UserEmail = System.Web.HttpContext.Current.User.Identity.Name;
            newPersonalTask.TaskIntiatorEmail = UserEmail;
            newPersonalTask.TaskStartDate = DateTime.Now;

            var Task_Owner = ent.Users.Where(a => a.Email == newPersonalTask.TaskOwnerEmail).FirstOrDefault();
            newPersonalTask.userId = Task_Owner.UserID;
            ent.PersonalTasks.Add(newPersonalTask);
            ent.SaveChanges();
            return RedirectToAction("TasksAndMeetings", "Home");
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}