using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace MeetingMinutesGP.signalr.hub
{
    public class MyHub1 : Hub
    {
        public async System.Threading.Tasks.Task GroupSendMessage(string user, string message, string groupName)
        {
            //Clients.Group(groupName).GroupReceiveMessage(user, message);
            //  await Clients.Group(groupName).SendAsync("GroupReceiveMessage", user, message);
            Clients.Group(groupName).GroupReceiveMessage(user, message);
        }

        public async System.Threading.Tasks.Task AddToGroup(string groupName)
        {
            await Groups.Add(Context.ConnectionId, groupName);
        }
    }
}