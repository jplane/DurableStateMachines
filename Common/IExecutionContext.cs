using System.Collections.Generic;
using System.Threading.Tasks;

namespace StateChartsDotNet.Common
{
    public interface IExecutionContext
    {
        IDictionary<string, object> Data { get; }

        Task SendStopMessageAsync();
        Task SendMessageAsync(string message, object content = null, IReadOnlyDictionary<string, object> parameters = null);

        Task StartAsync();
        Task WaitForCompletionAsync();
        Task StartAndWaitForCompletionAsync();
    }
}
