using StateChartsDotNet.Common.Model.Execution;
using System.Collections.Generic;

namespace StateChartsDotNet.Common.Model.States
{
    public interface ITransitionMetadata : IModelMetadata
    {
        IEnumerable<string> Targets { get; }
        IEnumerable<string> Messages { get; }
        TransitionType Type { get; }

        bool EvalCondition(dynamic data);
        IEnumerable<IExecutableContentMetadata> GetExecutableContent();
    }
}
