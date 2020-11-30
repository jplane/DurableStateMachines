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

        void ConfigureChildStateChart(IRootStateMetadata statechart);
        void ConfigureExternalQuery(string id, ExternalQueryDelegate handler);
        void ConfigureExternalService(string id, ExternalServiceDelegate handler);
        Task StopAsync();
        Task SendAsync(string message, object content = null, IReadOnlyDictionary<string, object> parameters = null);
    }
}
