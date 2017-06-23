using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace MESWeb.Hubs
{
    public class NotificationHub : Hub
    {
        public void Send(string mac, string machine, string atom, string worker, string state, string message)
        {
            // Call the addNewMessageToPage method to update clients.
            Clients.All.addNewMessageToPage(mac, machine, atom, worker, state, message);
        }

        public void ShowMessageServer(object message)
        {
            Clients.All.showMessageServer(message);
        }

    }
}