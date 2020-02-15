using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MeetingMinutesGP.Models;
using System.Net.Mail;
using System.Net;
using System.Web.Security;
using System.Text;

namespace MeetingMinutesGP.Controllers
{
    public class UserController : Controller
    {
        
        //Registration Action
        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }
        //Registration POST Action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration([Bind(Exclude = "Gender,Birthdate,Phone,Company,Position,ActivationCode,IsEmailVerified,ResetPasswordCode,ResetPasswordDate,navigateToResetPassword")]User user)
        {
            bool status = false;
            string message = "";
            //Model Validation
            if (ModelState.IsValid)
            {

                #region  //Email is already Exist
                var isExist = IsEmailExist(user.Email);
                if (isExist)
                {
                    ModelState.AddModelError("EmailExist", "Email aleardy exist");
                    return View(user);
                }
                #endregion
                #region //Generate Activation Code
                user.ActivationCode = Guid.NewGuid();
                #endregion
                #region //Password Hashing
                user.Password = Crypto.Hash(user.Password);
                user.ConfirmPassword = Crypto.Hash(user.ConfirmPassword);
                #endregion
                user.IsEmailVerified = false;
                #region //Save to Database and send Email to User
                using (GPEntities entity = new GPEntities())
                {
                    //Save to Database
                    entity.Users.Add(user);
                    entity.Configuration.ValidateOnSaveEnabled = false;
                    entity.SaveChanges();

                    //send Email to User
                    SendVerificationLinkEmail(user.Email, user.ActivationCode.ToString());
                    message = "Registeration successfully done. Account activation link " +
                                " has been sent to your email:" + user.Email;
                    status = true;
                }
                #endregion
            }
            else
            {
                message = "Invalid Request";
            }

            ViewBag.Message = message;
            ViewBag.Status = status;
            return View(user);
        }

        //Verify Account
        public ActionResult VerifyAccount(string id)
        {
            bool Status = false;
            using (GPEntities entity = new GPEntities())
            {
                entity.Configuration.ValidateOnSaveEnabled = false; // This line I have added here to avoid confirm
                                                                    // password does not match issuse on save changes
                var v = entity.Users.Where(a => a.ActivationCode == new Guid(id)).FirstOrDefault();
                if (v != null)
                {
                    v.IsEmailVerified = true;
                    entity.SaveChanges();
                    Status = true;
                }
                else
                {
                    ViewBag.Message = "Invalid Request";
                }
            }
            ViewBag.Status = Status;
            return View();
        }

        //Login
        [HttpGet]
        public ActionResult Login()
        {
            UserLogin userlogin = new UserLogin();
            HttpCookie saveLoginInfoCookie = Request.Cookies["User"];
            if(saveLoginInfoCookie != null)
            {
                userlogin.Email = saveLoginInfoCookie["Email"].ToString();

                //Decrypt Password
                string EncryptPassword = saveLoginInfoCookie["Password"].ToString();
                byte[] b = Convert.FromBase64String(EncryptPassword);
                string DecryptPassword = ASCIIEncoding.ASCII.GetString(b);

                userlogin.Password = DecryptPassword.ToString();
                userlogin.RememberMe = true;            }
            return View(userlogin);
        }

        //Login POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserLogin userlogin, string ReturnUrl = "")
        {
            string message = "";
            if (ModelState.IsValid)
            {
               HttpCookie saveLoginInfoCookie = new HttpCookie("User");
               if(userlogin.RememberMe == true)
                {
                    saveLoginInfoCookie["Email"] = userlogin.Email;

                    //Encrypt Password
                    byte[] b = ASCIIEncoding.ASCII.GetBytes(userlogin.Password);
                    string EncryptedPassword = Convert.ToBase64String(b);

                    saveLoginInfoCookie["Password"] = EncryptedPassword;
                    saveLoginInfoCookie.Expires = DateTime.Now.AddDays(2);
                    HttpContext.Response.Cookies.Add(saveLoginInfoCookie);
                }
                else
                {
                    saveLoginInfoCookie.Expires = DateTime.Now.AddDays(-1);
                    HttpContext.Response.Cookies.Add(saveLoginInfoCookie);
                }
                using (GPEntities entity = new GPEntities())
                {
                    var v = entity.Users.Where(a => a.Email == userlogin.Email).FirstOrDefault();
                    if (v != null)
                    {
                        if(v.IsEmailVerified == false)
                        {
                            message = "Your email is not verified. Check your email and verify it.";
                        }
                        else
                        {
                            if (string.Compare(Crypto.Hash(userlogin.Password), v.Password) == 0)
                            {
                                int timeout = userlogin.RememberMe ? 525600 : 20; // 525600 min = 1 year
                                var ticket = new FormsAuthenticationTicket(userlogin.Email, userlogin.RememberMe, timeout);
                                string encrypted = FormsAuthentication.Encrypt(ticket);
                                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                                cookie.Expires = DateTime.Now.AddMinutes(timeout);
                                cookie.HttpOnly = true;
                                Response.Cookies.Add(cookie);

                                if (Url.IsLocalUrl(ReturnUrl))
                                {
                                    return Redirect(ReturnUrl);
                                }
                                else
                                {
                                    //return RedirectToAction("Index", "Dashboard");
                                    return RedirectToAction("TasksAndMeetings", "Home");
                                }
                            }
                            else
                            {
                                message = "Invalid Email or Password.";
                            }
                        }
                        
                    }
                    else
                    {
                        message = "Invalid Email or Password.";
                    }
                }
            }
            else
            {
                message = "Invalid Request";
            }
            ViewBag.Message = message;
            return View();
        }

        //Logout
        [Authorize]
        [HttpPost]
        public ActionResult Logout()
        {
            // Delete unauthorized pages from back button after logout
            Session.Clear();
            Session.Abandon();
            Session.RemoveAll();

            //Logout
            FormsAuthentication.SignOut();

            // Delete unauthorized pages from back button after logout
            this.Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            this.Response.Cache.SetNoStore();

            return RedirectToAction("Login", "User");
        }

        //Check if User Email exist
        [NonAction]
        public bool IsEmailExist(string email)
        {
            using (GPEntities entity = new GPEntities())
            {
                var v = entity.Users.Where(a => a.Email == email).FirstOrDefault();
                return v != null;
            }
        }
        //send Email to User
        [NonAction]
        public void SendVerificationLinkEmail(string email, string activationCode, string emailFor = "verifyAccount")
        {
            var verifyURL = "/User/"+ emailFor + "/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyURL);
            var fromEmail = new MailAddress("meetingminutesgp@gmail.com", "MeetingMinutesGP");
            var toEmail = new MailAddress(email, "MeetingMinutesGP");
            var fromEmailPassword = "MeetingMinutesGP2019";
            string subject = "";
            string body = "";
            if (emailFor == "verifyAccount")
            {
                subject = "Your account is successfully created!";
                body = "<br/><br/>We are excited to tell you that your MeetingMinutes account is" +
                                " successfully created. Please click on the below link to verify your account" +
                                " <br/><br/><a href='" + link + "'>" + link + "</a>";
            }
            else if(emailFor == "ResetPassword")
            {
                subject = "Reset Password";
                body = "Hi, <br/><br/>We got request for reset password. Please click on the below link to reset your" +
                       "password <br/><br/><a href='" + link + "'>Reset Password link</a>";
            }
            
            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };
            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                smtp.Send(message);
        }

        //Forgot Password
        public ActionResult ForgotPassword()
        {
            return View();
        }
        //Forgot Password POST
        [HttpPost]
        public ActionResult ForgotPassword(String Email)
        {
            //Verify Email
            //Generate Reset Password Link
            //Send Email
            string message = "";
            bool Status = false;
            using(GPEntities entity = new GPEntities())
            {
                var account = entity.Users.Where(a => a.Email == Email).FirstOrDefault();
                if(account != null)
                {
                    //Send Email for Reset Password
                    account.ResetPasswordCode = Guid.NewGuid();
                    //This line to avoid cofirm password not match issue
                    entity.Configuration.ValidateOnSaveEnabled = false;
                    entity.SaveChanges();
                    SendVerificationLinkEmail(account.Email, account.ResetPasswordCode.ToString(), "ResetPassword");
                    message = "Reset password link has been sent to your email";
                }
                else
                {
                    message = "Account not found";
                }
            }
            ViewBag.Message = message;
            return View();
        }
        public ActionResult ResetPassword(string id)
        {
            //Verify the reset password link
            //Find account associated with this link
            //Redirect to reset password page
            using(GPEntities entity = new GPEntities())
            {
                var user = entity.Users.Where(a => a.ResetPasswordCode == new Guid(id)).FirstOrDefault();
                if(user != null)
                {
                    ResetPasswordModel model = new ResetPasswordModel();
                    model.ResetCode = id;
                    return View(model);
                }
                else
                {
                    return HttpNotFound();
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordModel model)
        {
            var message = "";
            if (ModelState.IsValid)
            {
                using (GPEntities entity = new GPEntities())
                {
                    var user = entity.Users.Where(a => a.ResetPasswordCode == new Guid(model.ResetCode)).FirstOrDefault();
                    if (user != null)
                    {
                        user.Password = Crypto.Hash(model.NewPassword);
                        user.ResetPasswordCode = null;
                        entity.Configuration.ValidateOnSaveEnabled = false;
                        entity.SaveChanges();
                        message = "New password updated successfully";
                    }
                }
            }
            else
            {
                message = "Somethis invalid";
            }
            ViewBag.Message = message;
            return View(model);
        }
    }
}