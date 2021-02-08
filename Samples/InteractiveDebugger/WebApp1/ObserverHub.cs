using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp1
{
    public sealed class ObserverHub : Hub
    {
        public Task Register(string instanceId)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, instanceId);
        }

        public Task Unregister(string instanceId)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, instanceId);
        }

        public Task Break(string instanceId, Dictionary<string, object> info)
        {
            return Clients.OthersInGroup(instanceId).SendAsync("break", info);
        }

        public Task Resume(string instanceId)
        {
            return Clients.OthersInGroup(instanceId).SendAsync("resume");
        }
    }
}
