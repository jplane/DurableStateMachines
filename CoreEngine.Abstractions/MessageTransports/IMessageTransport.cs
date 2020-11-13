using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.Abstractions.MessageTransports
{
    public interface IMessageTransport
    {
        Task SendAsync(Message message);

        Task<bool> HasMessageAsync();

        Task<bool> TryReceiveAsync(out Message message);
    }
}
