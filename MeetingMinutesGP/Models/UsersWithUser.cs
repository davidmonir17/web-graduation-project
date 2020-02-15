using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MeetingMinutesGP.Models
{
    public class UsersWithUser
    {
        public User CurrentUser;
        public List<User> RestOfUsers;
        public List<string> FollowerNotFollower;
        public List<FollowTableClass> FollowersTableData;
    }
}