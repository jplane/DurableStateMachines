using StateChartsDotNet.CoreEngine.Abstractions.Model.DataManipulation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.Abstractions.Model.Execution
{
    public interface ISendMessageMetadata : IExecutableContentMetadata
    {
        string Id { get; }
        string IdLocation { get; }

        Task<string> GetType(dynamic data);
        Task<TimeSpan> GetDelay(dynamic data);
        Task<string> GetTarget(dynamic data);
        Task<string> GetEvent(dynamic data);
        Task<IContentMetadata> GetContent();
        Task<IEnumerable<IParamMetadata>> GetParams();
    }
}
