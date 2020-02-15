using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MeetingMinutesGP.Models;
using System.Web.Mvc;

namespace MeetingMinutesGP.Controllers
{
    public class FollowController : Controller
    {
        // GET: Follow
        GPEntities db = new GPEntities();
        List<User> Users_list;
        User Current_user;
        [Authorize]
        public ActionResult All_Users()
        {
            string UserEmail = System.Web.HttpContext.Current.User.Identity.Name;
            Users_list = db.Users.ToList();
            Current_user = db.Users.Where(a => a.Email == UserEmail).FirstOrDefault();
            Users_list.Remove(Current_user);
            UsersWithUser User_Users = new UsersWithUser();
            User_Users.CurrentUser = Current_user;
            User_Users.RestOfUsers = Users_list;
            User_Users.FollowersTableData = new List<FollowTableClass>();
            foreach (var follow in db.GetFllowTableData())
            {
                FollowTableClass f = new FollowTableClass();
                f.followerId = follow.FollowerID;
                f.followedId = follow.FollowedID;
                User_Users.FollowersTableData.Add(f);
            }

            User_Users.FollowerNotFollower = new List<string>();
            bool flag;
            for (var index = 0; index < User_Users.RestOfUsers.Count; index++)
            {
                flag = false;
                for (int index2 = 0; index2 < User_Users.FollowersTableData.Count; index2++)
                {
                    if (Current_user.UserID == User_Users.FollowersTableData[index2].followerId && User_Users.RestOfUsers[index].UserID == User_Users.FollowersTableData[index2].followedId)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    User_Users.FollowerNotFollower.Add("Follower");
                }
                else
                {
                    User_Users.FollowerNotFollower.Add("Not Follower");
                }
            }
            return View(User_Users);

        }
        [Authorize]
        public ActionResult Follow_User(int id)
        {
            string UserEmail = System.Web.HttpContext.Current.User.Identity.Name;
            User follower = db.Users.Where(a => a.Email == UserEmail).FirstOrDefault();
            db.Users.FirstOrDefault(x => x.UserID == id).Users.Add(follower);
            db.SaveChanges();
            return RedirectToAction("All_Users");

        }
        [Authorize]
        public ActionResult Unfollow_User(int id)
        {
            string UserEmail = System.Web.HttpContext.Current.User.Identity.Name;
            User follower = db.Users.Where(a => a.Email == UserEmail).FirstOrDefault();
            db.Users.FirstOrDefault(x => x.UserID == id).Users.Remove(follower);
            db.SaveChanges();
            return RedirectToAction("All_Users");
        }
    }
}