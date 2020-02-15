using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MeetingMinutesGP.Models;

namespace MeetingMinutesGP.Controllers
{
    public class PostController : Controller
    {
        GPEntities db;
        // GET: Post
        //id Is user_id sent to create post using it
        [Authorize]
        public ActionResult create()
        {
            string UserEmail = System.Web.HttpContext.Current.User.Identity.Name;
            db = new GPEntities();
            var UserData = db.Users.Where(a => a.Email == UserEmail).FirstOrDefault();
            Post p = new Post();
            p.userID = UserData.UserID;
            return View(p);
        }
        //id Is user id to select specific posts of a spesifc user
        [Authorize]
        [HttpGet]
        public ActionResult UploadPostsFromDatabaseOfUser()
        {
            string UserEmail = System.Web.HttpContext.Current.User.Identity.Name;
            
            db = new GPEntities();
            var UserData = db.Users.Where(a => a.Email == UserEmail).FirstOrDefault();
            List<Post> post_Prelist = db.Posts.ToList();
            List<Post> post_Postlist = new List<Post>();
            for (int i = 0; i < post_Prelist.Count; i++)
            {
                if (post_Prelist[i].userID == UserData.UserID)
                {
                    post_Postlist.Add(post_Prelist[i]);
                }
            }
            UserPost posts = new UserPost();
            posts.user_name = db.Users.Where(a => a.UserID == UserData.UserID).FirstOrDefault();
            posts.user_posts = post_Postlist;
            return View(posts);
        }
        [Authorize]
        [HttpPost]
        public ActionResult create_post(Post post)
        {
            db = new GPEntities();
            if (post.PostID == 0)
            {
                if (post.PostContent == null || post.PostPrivacy == null)
                {
                    return RedirectToAction("UploadPostsFromDatabaseOfUser");
                }
                db.Posts.Add(post);
            }
            else
            {
                var Edited_Post = db.Posts.Where(a => a.PostID == post.PostID).FirstOrDefault();
                Edited_Post.PostContent = post.PostContent;
                Edited_Post.PostPrivacy = post.PostPrivacy;
            }
            db.SaveChanges();
            return RedirectToAction("UploadPostsFromDatabaseOfUser");

        }
        //id Is post id to delete specific post based on user choice
        [Authorize]
        public ActionResult Delete_post(int id)
        {
            db = new GPEntities();
            Post deleted_post = db.Posts.Where(a => a.PostID == id).FirstOrDefault();
            db.Posts.Remove(deleted_post);
            db.SaveChanges();
            return RedirectToAction("UploadPostsFromDatabaseOfUser");
        }
        //id Is post id to edit specific post
        [Authorize]
        public ActionResult Edit_post(string id)
        {
            byte[] encoded = Convert.FromBase64String(id);
            int Decodedid = int.Parse(System.Text.Encoding.UTF8.GetString(encoded));
            db = new GPEntities();
            Post Edited_post = db.Posts.Where(a => a.PostID == Decodedid).FirstOrDefault();
            return View("create", Edited_post);
        }
        [Authorize]
        public ActionResult PublicPostsForFollower(string id)
        {
            byte[] encoded = Convert.FromBase64String(id);
            int Decodedid = int.Parse(System.Text.Encoding.UTF8.GetString(encoded));

            string UserEmail = System.Web.HttpContext.Current.User.Identity.Name;
            db = new GPEntities();
            var UserData = db.Users.Where(a => a.Email == UserEmail).FirstOrDefault();
            UsersWithUser User_Users = new UsersWithUser();
            User_Users.FollowersTableData = new List<FollowTableClass>();
            foreach (var follow in db.GetFllowTableData())
            {
                FollowTableClass f = new FollowTableClass();
                f.followerId = follow.FollowerID;
                f.followedId = follow.FollowedID;
                User_Users.FollowersTableData.Add(f);
            }
            bool flag = false;
            for (int i = 0; i < User_Users.FollowersTableData.Count; i++)
            {

                if (User_Users.FollowersTableData[i].followerId == UserData.UserID && User_Users.FollowersTableData[i].followedId == Decodedid)
                {
                    flag = true;
                    break;
                }
            }
            UserPost posts = new UserPost();
            posts.user_posts = new List<Post>();
            if (flag)
            {
                var PostsOfFollowedPerson = db.Posts.ToList();
                for (int i = 0; i < PostsOfFollowedPerson.Count; i++)
                {
                    if (PostsOfFollowedPerson[i].PostPrivacy == "public")
                    {
                        if (PostsOfFollowedPerson[i].userID == Decodedid)
                        {
                            posts.user_posts.Add(PostsOfFollowedPerson[i]);
                        }
                    }
                }
                posts.user_name = db.Users.Where(a => a.UserID == Decodedid).FirstOrDefault();
                return View(posts);
            }
      
            return RedirectToAction("All_Users", "Follow");
        }
    }
}