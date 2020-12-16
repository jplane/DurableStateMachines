using StateChartsDotNet.Common.Messages;
using StateChartsDotNet.Common.Model.States;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StateChartsDotNet.Common
{
    public interface IInstanceManager
    {
        Task StartAsync();
        Task StartAsync(CancellationToken token);
        Task WaitForCompletionAsync();
        Task WaitForCompletionAsync(CancellationToken token);
        Task StartAndWaitForCompletionAsync();
        Task StartAndWaitForCompletionAsync(CancellationToken token);
    }
}
