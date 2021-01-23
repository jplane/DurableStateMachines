using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Debugger
{
    public class DebugHub : Hub
    {
        public Task Break(Dictionary<string, object> info)
        {
            return Clients.Others.SendAsync("break", info);
        }

        public Task Resume()
        {
            return Clients.Others.SendAsync("resume");
        }
    }
}
