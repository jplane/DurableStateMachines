using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HtmlDebugger
{
    public class DebugHub : Hub
    {
        public Task Register(string instanceId)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, instanceId);
        }

        public Task UnRegister(string instanceId)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, instanceId);
        }

        public Task Break(string instanceId, Dictionary<string, object> info)
        {
            return Clients.Group(instanceId).SendAsync("break", info);
        }

        public Task Resume(string instanceId)
        {
            return Clients.Group(instanceId).SendAsync("resume");
        }
    }
}
