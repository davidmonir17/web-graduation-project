using MeetingMinutesGP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MeetingMinutesGP.Controllers
{
    public class StatisticsController : Controller
    {
        // GET: Statistics
        GPEntities _context = new GPEntities();

        public ActionResult Index()
        {
            return View();
        }
        [Route("stat/tasks/{s1}/{s2}")]
        public ActionResult tasks(DateTime s1, DateTime s2)
        {//&& c.TaskStatus == "not started"
            string UserEmail = System.Web.HttpContext.Current.User.Identity.Name;
            _context = new GPEntities();
            var UserData = _context.Users.Where(a => a.Email == UserEmail).FirstOrDefault();
            int id = UserData.UserID;

            var st = _context.UserMeetings.Where(c => c.TaskStartDate >= s1 && c.TaskEndDate <= s2 && c.userID == id && c.TaskStatus == "not started").ToList();
            int nstart = st.Count();

            st = _context.UserMeetings.Where(c => c.TaskStartDate >= s1 && c.TaskEndDate < s2 && c.userID == id && c.TaskStatus == "in prograss").ToList();
            int prograss = st.Count();

            st = _context.UserMeetings.Where(c => c.TaskStartDate >= s1 && c.TaskEndDate < s2 && c.userID == id && c.TaskStatus == "completed").ToList();
            int completed = st.Count();
            st = _context.UserMeetings.Where(c => c.TaskStartDate >= s1 && c.TaskEndDate < s2 && c.userID == id && c.TaskStatus == "cancelled").ToList();
            int cancelled = st.Count();
            st = _context.UserMeetings.Where(c => c.TaskStartDate >= s1 && c.TaskEndDate < s2 && c.userID == id && c.TaskStatus == "on hold").ToList();
            int hold = st.Count();
            st = _context.UserMeetings.Where(c => c.TaskStartDate >= s1 && c.TaskEndDate < s2 && c.userID == id && c.TaskStatus == "overdue").ToList();
            int overdue = st.Count();
            ViewData["total "] = nstart + prograss + completed + cancelled + hold + overdue;
            ViewData["nstart"] = nstart;
            ViewData["prograss"] = prograss;
            ViewData["completed"] = completed;
            ViewData["cancelled"] = cancelled;
            ViewData["hold"] = hold;
            ViewData["overdue"] = overdue;
            ViewData["va1"] = s1;
            ViewData["va2"] = s2;

            return View();
        }
        [Route("stat/opentasks/{s1}/{s2}")]
        public ActionResult opentasks(DateTime s1, DateTime s2)
        {

            string UserEmail = System.Web.HttpContext.Current.User.Identity.Name;
            _context = new GPEntities();
            var UserData = _context.Users.Where(a => a.Email == UserEmail).FirstOrDefault();
            int id = UserData.UserID;
            var st = _context.UserMeetings.Where(c => c.TaskStartDate >= s1 && c.TaskEndDate < s2 && c.userID == id).ToList();
            int ds = st.Count();
            ViewData["in time"] = ds;
            st = _context.UserMeetings.Where(c => c.TaskEndDate >= s1 && c.TaskEndDate < s2 && c.userID == id).ToList();
            ViewData["overdue"] = st.Count();

            ViewData["total"] = st.Count() + ds;


            return View();
        }
        [Route("stat/meeted/{s1}/{s2}")]
        public ActionResult meeted(DateTime s1, DateTime s2)
        {
            var st = _context.Meetings.Where(c => c.MeetingDate > s1 && c.MeetingDate < s2).ToList();
            int ds = st.Count();
            ViewData["num"] = ds;


            return View();
        }
        [Route("stat/meetingatt/{s1}/{s2}")]
        public ActionResult meetingatt(DateTime s1, DateTime s2)
        {
           
            string UserEmail = System.Web.HttpContext.Current.User.Identity.Name;
            _context = new GPEntities();
            var UserData = _context.Users.Where(a => a.Email == UserEmail).FirstOrDefault();
            int id = UserData.UserID;
            var st1 = _context.UserMeetings.Where(c=>c.userID == id).ToList();
            int dss = st1.Count();
            var st = _context.UserMeetings.Where(c=>c.Attended==true && c.userID==id).ToList();
            int ds = st.Count();
            ViewData["true"] = ds;
            ViewData["false"] = dss-ds;

            ViewData["total"] = dss ;

            return View();
        }



        /// <summary>
        /// //////////////////////////////////////////////
        [Route("stat/topics_stat/{s1}/{s2}/{want}")]
        public ActionResult topics_stat(DateTime s1, DateTime s2, string want = "medical")
        {
            want = want.ToLower();
            List<ListStatistic> m = new List<ListStatistic>();
            if (want != "")
            {

                //list<int> Meetingpourpose = new List<int>();
                //List<int> topicswanted = new List<int>();
                var st = _context.Meetings.Where(c => c.MeetingDate > s1 && c.MeetingDate < s2).ToList();

                for (int i = 0; i < st.Count(); i++)
                {
                    var stid = st[i].MeetingID;
                    var Agendanum = _context.Agenda.SingleOrDefault(c => c.meetingID == stid);
                    int x = 0;
                    var a = Agendanum.AgendaID;
                    var TopicInList = _context.Topics.Where(c => c.agendaId ==a).ToList();
                    for (int j = 0; j < TopicInList.Count(); j++)
                    {
                        string names = TopicInList[j].ListOfItems;
                        if (names != null)
                        {
                            string[] words = names.Split(',');
                            Array.Sort(words);
                            int yes = 5000000;
                            yes = Array.BinarySearch<string>(words, want);
                            if (yes != -1)
                            {
                                ListStatistic c11 = new ListStatistic();
                                //topicswanted.Add(TopicInList[j].TopicID);
                                //Meetingpourpose.Add(st[i].MeetingID);

                                /*     int x1 = st[i].MeetingID;
                                     string x2 = TopicInList[j].TopicName;
                                     string x3 = TopicInList[j].TopicDescription;*/
                                c11.Meetingpourpose = st[i].MeetingID;
                                c11.topicswanted = TopicInList[j].TopicName;
                                c11.topicswantedDiscribtion = TopicInList[j].TopicDescription;
                                /*    m.Meetingpourpose.Add(x1);
                                    m.topicswanted.Add(x2);
                                    m.topicswantedDiscribtion.Add(x3);*/
                                m.Add(c11);
                            }
                        }
                    }
                }

                return View(m);
            }




            return View();
        }
        /// /////////////////////////////////////////////
        /// </summary>
        /// <returns></returns>


        public ActionResult startstat()
        {
            return View();
        }
    }
}