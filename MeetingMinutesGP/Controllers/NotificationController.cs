using MeetingMinutesGP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MeetingMinutesGP.Controllers
{
    public class NotificationController : Controller
    {
        // GET: Notification
        //[Authorize]
        //public ActionResult Index()
        //{
        //    return View();
        //}
        public ActionResult EditISReaded(int id)
        {
            using (GPEntities entity = new GPEntities())
            {
                var v = entity.Notifications.Where(a => a.NotificaionId == id).FirstOrDefault();
                if (v != null)
                {
                    v.IsRead = true;

                }
                entity.Entry(v).State = System.Data.Entity.EntityState.Modified;
                entity.Configuration.ValidateOnSaveEnabled = false;
                entity.SaveChanges();
            }
            return RedirectToAction("Index", "Dashboard");
        }
        public ActionResult DeleteNotfication(int id)
        {
            using (GPEntities entity = new GPEntities())
            {
                var v = entity.Notifications.Where(a => a.NotificaionId == id).FirstOrDefault();
                if (v != null)
                {
                    entity.Notifications.Remove(v);
                    entity.SaveChanges();

                }
            }
            return RedirectToAction("Index", "Dashboard");
        }
        public JsonResult GetNotification()
        {
            return Json(NotificaionService.GetNotification(), JsonRequestBehavior.AllowGet);

        }
    }
}