using StateChartsDotNet.Common.Messages;
using StateChartsDotNet.Common.Model.States;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StateChartsDotNet.Common
{
    public interface IExecutionContext
    {
        bool IsRunning { get; }
        object this[string key] { get; set; }

        Task StopAsync();
        Task SendAsync(ExternalMessage message);
        Task SendAsync(string message, object content = null, IReadOnlyDictionary<string, object> parameters = null);
    }
}
