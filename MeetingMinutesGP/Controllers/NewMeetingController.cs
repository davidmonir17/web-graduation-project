using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MeetingMinutesGP.Models;
using System.IO;
using Rotativa;

namespace MeetingMinutesGP.Controllers
{
    public class NewMeetingController : Controller
    {
        // GET: NewMeeting
        [Authorize]
        public ActionResult Meeting_Form()
        {

            GPEntities ent = new GPEntities();
            MeetingMinutes MeetIn = new MeetingMinutes();
            var userLst = ent.Users.ToList();
            MeetIn.users = userLst;
            var locLst = ent.Locations.ToList();

            MeetIn.UserEmail = System.Web.HttpContext.Current.User.Identity.Name;
            var Current_User = ent.Users.Where(a => a.Email == MeetIn.UserEmail).FirstOrDefault();

            MeetIn.company = new List<string>();
            MeetIn.company.Add(Current_User.Company);

            List<string> locNames = new List<string>();
            for (var i = 0; i < locLst.Count; i++)
            {
                if (locLst[i].Company == MeetIn.company[0])
                {
                    locNames.Add(locLst[i].Address);
                }
            }
            MeetIn.addresses = locNames.Distinct().ToList();

            return View(MeetIn);
        }

        [Authorize]
        [HttpPost]
        public ActionResult createMeeting(MeetingMinutes MeetIn)
        {
            GPEntities ent = new GPEntities();
            if (MeetIn.location.Address != null)
            {
                var locs = ent.Locations.ToList();

                var locID = 0;
                for (var i = 0; i < locs.Count; i++)
                {
                    if (locs[i].Company == MeetIn.location.Company && locs[i].Address == MeetIn.location.Address && locs[i].Floor == MeetIn.location.Floor && locs[i].RoomNumber == MeetIn.location.RoomNumber)
                    {
                        locID = locs[i].LocationID;
                        MeetIn.meeting.locationID = locID;
                        break;
                    }
                }
                if (locID == 0)
                {
                    ent.Locations.Add(MeetIn.location);
                    ent.SaveChanges();
                    MeetIn.meeting.locationID = MeetIn.location.LocationID;
                }
            }

            MeetIn.meeting.Status = "In progress";
            ent.Meetings.Add(MeetIn.meeting);
            ent.SaveChanges();

            MeetIn.agenda.meetingID = MeetIn.meeting.MeetingID;
            ent.Agenda.Add(MeetIn.agenda);
            ent.SaveChanges();

            var filesCount = Request.Files.Count;
            for (int i = 0; i < MeetIn.topics.Count; i++)
            {
                MeetIn.topics[i].agendaId = MeetIn.agenda.AgendaID;
                if (MeetIn.topics[i].subTopicId != null)
                {
                    MeetIn.topics[i].subTopicId = MeetIn.topics[(int)MeetIn.topics[i].subTopicId].TopicID;
                }
                




                string id = MeetIn.topics[i].TopicName.ToLower();
                string[] arrToCheck = new string[] { "ourselves", "hers", "between", "yourself", "but", "again", "there", "about", "once", "during", "out", "very", "having", "with", "they", "own", "an", "be", "some", "for", "do", "its", "yours", "such", "into", "of", "most", "itself", "other", "off", "is", "s", "am", "or", "who", "as", "from", "him", "each", "the", "themselves", "until", "below", "are", "we", "these", "your", "his", "through", "don", "nor", "me", "were", "her", "more", "himself", "this", "down", "should", "our", "their", "while", "above", "both", "up", "to", "ours", "had", "she", "all", "no", "when", "at", "any", "before", "them", "same", "and", "been", "have", "in", "will", "on", "does", "yourselves", "then", "that", "because", "what", "over", "why", "so", "can", "did", "not", "now", "under", "he", "you", "herself", "has", "just", "where", "too", "only", "myself", "which", "those", "i", "after", "few", "whom", "t", "being", "if", "theirs", "my", "against", "a", "by", "doing", "it", "how", "further", "was", "here", "than", "" };
                string input = (id);
                string[] arr = input.Split(' ');
                List<string> AfterRemovingPunc = new List<string>();
                string output = "";
                bool flag = false;
                foreach (string value in arr)
                {
                    AfterRemovingPunc.Add(TrimPunctuation(value));
                }
                for (int ii = 0; ii < AfterRemovingPunc.Count; ii++)
                {
                    if (!arrToCheck.Contains(AfterRemovingPunc[ii]))
                    {

                        if (flag == false)
                        {
                            output = output + AfterRemovingPunc[ii];
                            flag = true;
                        }
                        else
                        {
                            output = output + " " + AfterRemovingPunc[ii];
                        }
                    }
                }
                string[] TopicWords = output.Split(' ');
                TopicWords = TopicWords.Distinct().ToArray();
                MeetIn.topics[i].TopicImpWords = string.Join(" ", TopicWords);

                ent.Topics.Add(MeetIn.topics[i]);
                ent.SaveChanges();

                if (filesCount > 0 && Request.Files[i].ContentLength > 0)
                {
                    var file = Request.Files[i];
                    // extract only the filename
                    var fileName = Path.GetFileName(file.FileName);
                    // store the file inside ~/App_Data/uploads folder
                    var topics = ent.Topics.ToList();
                    var folderName = topics.Last().TopicID.ToString();
                    var pathString = System.IO.Path.Combine(Server.MapPath("~/App_Data/uploads/Topic_Files"), folderName);
                    System.IO.Directory.CreateDirectory(pathString);
                    string path = Path.Combine(pathString, fileName);
                    file.SaveAs(path);
                    MeetIn.topics[i].FileLocation = path;
                    filesCount--;
                }

                ent.SaveChanges();

            }

            var Current_User = ent.Users.Where(a => a.Email == MeetIn.UserEmail).FirstOrDefault();
            MeetIn.user_meeting = new UserMeeting();
            MeetIn.user_meeting.userID = Current_User.UserID;
            MeetIn.user_meeting.meetingID = MeetIn.meeting.MeetingID;
            MeetIn.user_meeting.Attended = true;
            MeetIn.user_meeting.Coordinator = true;
            //MeetIn.user_meeting.TaskIntiatorEmail = Current_User.Email.ToString();
            ent.UserMeetings.Add(MeetIn.user_meeting);

            ent.SaveChanges();


            Notification notifiCurrent_User = new Notification();
            notifiCurrent_User.Date = MeetIn.meeting.MeetingDate;
            notifiCurrent_User.IsRead = false;
            notifiCurrent_User.UserId = Current_User.UserID;
            notifiCurrent_User.MeetingId = MeetIn.meeting.MeetingID;
            notifiCurrent_User.Message = "You have a new meeting ( " + MeetIn.agenda.Title + " )";
            notifiCurrent_User.DetailsURL = "CurrMeeting/viewMeetingContent";
            //notifiCurrent_User.VoteID = 0;
            ent.Notifications.Add(notifiCurrent_User);
            ent.SaveChanges();

            var users = ent.Users.ToList();
            for (int j = 0; j < MeetIn.UserMeeting.Count; j++)
            {
                for (int i = 0; i < users.Count; i++)
                {
                    if (users[i].Email == MeetIn.UserMeeting[j].Email)
                    {
                        /*MeetIn.user_meeting = new UserMeeting();
                        MeetIn.user_meeting.userID = users[i].UserID;

                        MeetIn.user_meeting.meetingID = MeetIn.meeting.MeetingID;

                        MeetIn.user_meeting.Attended = false;

                        ent.UserMeetings.Add(MeetIn.user_meeting);
                        ent.SaveChanges();*/
                        UserMeeting user_meeting = new UserMeeting();
                        user_meeting.userID = users[i].UserID;
                        user_meeting.meetingID = MeetIn.meeting.MeetingID;
                        user_meeting.Attended = false;
                        ent.UserMeetings.Add(user_meeting);
                        ent.SaveChanges();


                        Notification notifi = new Notification();
                        notifi.Date = MeetIn.meeting.MeetingDate;
                        notifi.IsRead = false;
                        notifi.UserId = users[i].UserID;
                        notifi.MeetingId = MeetIn.meeting.MeetingID;
                        notifi.Message = "You are invited to a new meeting ( " + MeetIn.agenda.Title + " )";
                        notifi.DetailsURL = "CurrMeeting/viewMeetingContent";
                        //notifi.VoteID = 0;
                        ent.Notifications.Add(notifi);
                        ent.SaveChanges();
                    }
                }

            }
            return RedirectToAction("TemplateOrNot", new{id= MeetIn.meeting.MeetingID });
            //return RedirectToAction("TasksAndMeetings", "Home");
        }
        public ActionResult TemplateOrNot(int id)
        {
            Meeting meetin = new Meeting();
            meetin.MeetingID = id;
            return View(meetin);
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

        public ActionResult maketamplate()
        {


            GPEntities ent = new GPEntities();
            List<Topic> T = new List<Topic>();
            var agendas = ent.Agenda.ToList();
            ViewData["total"] = agendas.Last().AgendaID;
            int id = agendas.Last().AgendaID;
            var topics = ent.Topics.Where(C => C.agendaId == id).ToList();
            T = topics;
            return View(T);
        }
        public ActionResult EatTemplate(int id)
        {
           

            GPEntities ent = new GPEntities();
            List<Topic> T = new List<Topic>();
            Meeting meeting = new Meeting();
            Location loc = new Location();
            meeting = ent.Meetings.Where(d => d.MeetingID == id).SingleOrDefault();
            int MeetingId = meeting.MeetingID;
            ViewData["total1"] = MeetingId;
            ViewData["MeetingDuration"] = meeting.MeetingDuration;
            ViewData["MeetingDate"] = meeting.MeetingDate;
            if (loc.Address != null)
            {
                ViewData["MeetingLocation"] = loc.Address;
            }
            loc = ent.Locations.Where(C => C.LocationID == meeting.locationID).SingleOrDefault();
            var agenda = ent.Agenda.Where(C => C.meetingID == MeetingId).SingleOrDefault();
            int agid = agenda.AgendaID;
            var topics = ent.Topics.Where(C => C.agendaId == agid).ToList();
            T = topics;
            
            return View(T);
        }
        public ActionResult maketamplate2()
        {


            GPEntities ent = new GPEntities();
            List<Topic> T = new List<Topic>();
            var agendas = ent.Agenda.ToList();
            ViewData["total"] = agendas.Last().AgendaID;
            int id = agendas.Last().AgendaID;
            var topics = ent.Topics.Where(C => C.agendaId == id).ToList();
            T = topics;
            return View(T);
        }
        public ActionResult medical_tamplete(int id)
        {
            //GPEntities ent = new GPEntities();
            //List<Topic> T = new List<Topic>();
            //Meeting meeting = new Meeting();
            //Location loc = new Location();
            //var agendas = ent.Agenda.ToList();
            //ViewData["total"] = agendas.Last().Title;

            //int MeetingId = agendas.Last().meetingID;
            //ViewData["total1"] = MeetingId;
            //meeting = ent.Meetings.Where(d => d.MeetingID == MeetingId).SingleOrDefault();

            //ViewData["MeetingDuration"] = meeting.MeetingDuration;
            //ViewData["MeetingDate"] = meeting.MeetingDate;
            //loc = ent.Locations.Where(C => C.LocationID == meeting.locationID).SingleOrDefault();

            ////int id = agendas.Last().AgendaID;
            //var topics = ent.Topics.Where(C => C.agendaId == id).ToList();
            //T = topics;
            //return View(T);

            GPEntities ent = new GPEntities();
            List<Topic> T = new List<Topic>();
            Meeting meeting = new Meeting();
            Location loc = new Location();
            meeting = ent.Meetings.Where(d => d.MeetingID == id).SingleOrDefault();
            int MeetingId = meeting.MeetingID;
            ViewData["total1"] = MeetingId;
            ViewData["MeetingDuration"] = meeting.MeetingDuration;
            ViewData["MeetingDate"] = meeting.MeetingDate;
            if (loc.Address != null)
            {
                ViewData["MeetingLocation"] = loc.Address;
            }
            loc = ent.Locations.Where(C => C.LocationID == meeting.locationID).SingleOrDefault();
            var agenda = ent.Agenda.Where(C => C.meetingID == MeetingId).SingleOrDefault();
            int agid = agenda.AgendaID;
            var topics = ent.Topics.Where(C => C.agendaId == agid).ToList();
            T = topics;
            return View(T);
        }
        public ActionResult ReportTemplate()
        {


            GPEntities ent = new GPEntities();
            List<Topic> T = new List<Topic>();
            Meeting meeting = new Meeting();
            Location loc = new Location();
            var agendas = ent.Agenda.ToList();
            ViewData["total"] = agendas.Last().Title;

            int MeetingId = agendas.Last().meetingID;
            ViewData["total1"] = MeetingId;
            meeting = ent.Meetings.Where(d => d.MeetingID == MeetingId).SingleOrDefault();

            ViewData["MeetingDuration"] = meeting.MeetingDuration;
            ViewData["MeetingDate"] = meeting.MeetingDate;
            loc = ent.Locations.Where(C => C.LocationID == meeting.locationID).SingleOrDefault();
            ViewData["MeetingLocation"] = loc.Address;
            int id = agendas.Last().AgendaID;
            var topics = ent.Topics.Where(C => C.agendaId == id).ToList();
            T = topics;
            return View(T);
        }
        public ActionResult chosetamplate()
        {
            /* if (i == 1)
             {
                 return RedirectToAction("maketamplate", "NewMeeting");
             }
             else if (i == 2)
             {
                 return RedirectToAction("maketamplate2", "NewMeeting");

             }
             else
             {
                 return View();
             }*/
            return View();
        }
          public ActionResult exportpage(string id)
          {
            return new ActionAsPdf("EatTemplate",new { id = int.Parse(id) })
            {
                FileName = Server.MapPath("EatTemplate.pdf")
            };


            //GPEntities db = new GPEntities();
            //var agnda = db.Agenda.Where(a => a.meetingID == id).FirstOrDefault();
            ////khaled
            //return new Rotativa.ActionAsPdf("ExportRotitva", new { id = id })
            //{
            //    //FileName = agnda.Title + ".pdf",
            //    //PageOrientation = Rotativa.Options.Orientation.Landscape,
            //    //PageSize = Rotativa.Options.Size.A4
            //    //PageOrientation = Rotativa.Options.Orientation.Portrait

            //};
        }
        public ActionResult exportpage2(string id)
          {
              return new ActionAsPdf("medical_tamplete", new { id = int.Parse(id) })
              {
                  FileName = Server.MapPath("medical.pdf")
              };

          }

    }
}