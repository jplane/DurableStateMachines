using StateChartsDotNet.Common.Messages;
using StateChartsDotNet.Common.Model.States;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StateChartsDotNet.Common
{
    public interface ISessionManager
    {
        Task StartAsync();
        Task StopAsync();

        Task<string> StartInstanceAsync(IRootStateMetadata metadata);
        Task<string> StartInstanceAsync(IRootStateMetadata metadata, CancellationToken token);
        Task WaitForInstanceCompletionAsync(string instanceId);
        Task WaitForInstanceCompletionAsync(string instanceId, CancellationToken token);
    }
}
