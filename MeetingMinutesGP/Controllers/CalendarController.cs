using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


using DHTMLX.Scheduler;
using DHTMLX.Common;
using DHTMLX.Scheduler.Data;
using DHTMLX.Scheduler.Controls;

using MeetingMinutesGP.Models;
namespace MeetingMinutesGP.Controllers
{
    public class CalendarController : Controller
    {
        public ActionResult Index(string id)
        {
            byte[] encoded = Convert.FromBase64String(id);
            int Decodedid = int.Parse(System.Text.Encoding.UTF8.GetString(encoded));
            Session["TaskOrMeeting"] = Decodedid;
            var scheduler = new DHXScheduler(this);
            scheduler.InitialDate = DateTime.Now;
            scheduler.LoadData = true;
            scheduler.EnableDataprocessor = true;
            scheduler.BeforeInit.Add(string.Format("initResponsive({0})", scheduler.Name));

            return View(scheduler);
        }
        GPEntities db = new GPEntities();
        public ContentResult Data()
        {
            int MeetingOrTask = int.Parse(Session["TaskOrMeeting"].ToString());
            List<CalendarData> CalendarData = new List<CalendarData>();
            if (MeetingOrTask == 1)
            {
                string UserEmail = System.Web.HttpContext.Current.User.Identity.Name;
                User Current_user = db.Users.Where(a => a.Email == UserEmail).FirstOrDefault();
                List<UserMeeting> user_meeting = db.UserMeetings.ToList();
                for (int i = 0; i < db.UserMeetings.ToList().Count; i++)
                {
                    if (user_meeting[i].TaskStartDate != null && user_meeting[i].TaskEndDate != null && user_meeting[i].TaskIntiatorEmail==UserEmail)
                    {
                        CalendarData d = new Models.CalendarData();
                        d.id = user_meeting[i].CalendarTaskID;
                        d.text = user_meeting[i].AssignedTask;
                        d.start_date = (DateTime)user_meeting[i].TaskStartDate;
                        d.end_date = (DateTime)user_meeting[i].TaskEndDate;
                        CalendarData.Add(d);
                    }
                }
            }
            var calendarEvent = new List<CalendarEvent>();
            for(int i = 0; i < CalendarData.Count; i++)
            {
                CalendarEvent E = new CalendarEvent();
                E.id = CalendarData[i].id;
                E.text = CalendarData[i].text;
                E.start_date = CalendarData[i].start_date;
                E.end_date = CalendarData[i].end_date;
                calendarEvent.Add(E);
            }
            var data = new SchedulerAjaxData(calendarEvent);
            return (ContentResult)data;
        }

        public ContentResult Save(int? id, FormCollection actionValues)
        {
            var action = new DataAction(actionValues);
            long calendarID = action.SourceId;
            GPEntities db = new GPEntities();
            int MeetingOrTask = int.Parse(Session["TaskOrMeeting"].ToString());
            if (MeetingOrTask == 1)
            {
                try
                {
                    switch (action.Type)
                    {
                        case DataActionTypes.Insert:
                            //do insert
                            // action.TargetId = changedEvent.id;//assign postoperational id
                            break;
                            case DataActionTypes.Delete:
                                UserMeeting MeetingTask2 = db.UserMeetings.Where(a => a.CalendarTaskID == calendarID).SingleOrDefault();
                                db.UserMeetings.Remove(MeetingTask2);
                                db.SaveChanges();
                                break;
                             default:
                                var changedTask = (CalendarEvent)DHXEventsHelper.Bind(typeof(CalendarEvent), actionValues);
                                UserMeeting MeetingTask = db.UserMeetings.Where(a => a.CalendarTaskID == calendarID).SingleOrDefault();
                                MeetingTask.AssignedTask = changedTask.text;
                                MeetingTask.TaskStartDate = changedTask.start_date;
                                MeetingTask.TaskEndDate = changedTask.end_date;
                               db.SaveChanges();
                               break;
                    }
                }
                catch
                {
                    action.Type = DataActionTypes.Error;
                }
            }
            return (ContentResult)new AjaxSaveResponse(action);
        }
    }
}

