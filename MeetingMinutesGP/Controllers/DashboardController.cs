using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MeetingMinutesGP.Models;

namespace MeetingMinutesGP.Controllers
{
    public class DashboardController : Controller
    {
        // GET: Dashboard
        [Authorize]
        public ActionResult Index()
        {
            //        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]

            return View();
        }
        [Authorize]
        public ActionResult AccountSettings()
        {
            AccountSettingsModel UserAccountSettings = new AccountSettingsModel();
            using (GPEntities entity = new GPEntities())
            {
                var v = entity.Users.Where(a => a.Email == System.Web.HttpContext.Current.User.Identity.Name).FirstOrDefault();
                if (v != null)
                {
                    UserAccountSettings.Name = v.Name;
                    if (v.Gender != null)
                    {
                        if (v.Gender == true)
                        {
                            UserAccountSettings.Gender = "Male";

                        }
                        else if (v.Gender == false)
                        {
                            UserAccountSettings.Gender = "Female";

                        }
                    }
                    if (v.Birthdate != null)
                    {
                        UserAccountSettings.Birthdate = v.Birthdate;
                    }

                    if (v.Phone != null)
                    {
                        UserAccountSettings.Phone = v.Phone;
                    }

                    if (v.Company != null)
                    {
                        UserAccountSettings.Company = v.Company;
                    }
                    if (v.Position != null)
                    {
                        UserAccountSettings.Position = v.Position;
                    }
                }

            }
            return View(UserAccountSettings);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AccountSettings(AccountSettingsModel model)
        {
            var message = "";
            if (ModelState.IsValid)
            {
                using (GPEntities entity = new GPEntities())
                {
                    var v = entity.Users.Where(a => a.Email == System.Web.HttpContext.Current.User.Identity.Name).FirstOrDefault();
                    if (v != null)
                    {
                        if(model.Name != null)
                        {
                            v.Name = model.Name;
                        }
                        if (model.Gender != null)
                        {
                            if(model.Gender == "Male")
                            {
                                v.Gender = true;
                            }
                            else if (model.Gender == "Female")
                            {
                                v.Gender = false;
                            }
                        }
                        if (model.Birthdate != null)
                        {
                            v.Birthdate = model.Birthdate;
                        }
                        if (model.Phone != null)
                        {
                            v.Phone = model.Phone;
                        }
                        if (model.Company != null)
                        {
                            v.Company = model.Company;
                        }
                        if (model.Position != null)
                        {
                            v.Position = model.Position;
                        }
                        if (model.NewPassword != null)
                        {
                            v.Password = Crypto.Hash(model.NewPassword);
                            //v.ConfirmPassword = Crypto.Hash(model.ConfirmPassword);
                        }
                        entity.Entry(v).State = System.Data.Entity.EntityState.Modified;
                        entity.Configuration.ValidateOnSaveEnabled = false;
                        entity.SaveChanges();
                        message = "Your data updated successfully";
                    }

                }
            }
            ViewBag.Message = message;
            return View(model);
        }
    }
}