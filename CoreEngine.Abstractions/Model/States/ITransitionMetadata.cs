using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.Abstractions.Model.States
{
    public interface ITransitionMetadata
    {
        IEnumerable<string> Targets { get; }
        IEnumerable<string> Messages { get; }
        TransitionType Type { get; }

        bool EvalCondition(dynamic data);
        IEnumerable<IExecutableContentMetadata> GetExecutableContent();
    }
}
