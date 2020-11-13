using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.Abstractions.Model.States
{
    public interface ITransitionMetadata
    {
        IEnumerable<string> Targets { get; }
        IEnumerable<string> Events { get; }
        TransitionType Type { get; }

        Task<bool> EvalCondition(dynamic data);
        Task<IEnumerable<IExecutableContentMetadata>> GetExecutableContent();
    }
}
