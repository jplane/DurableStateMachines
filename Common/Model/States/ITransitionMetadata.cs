using StateChartsDotNet.Common.Model.Execution;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StateChartsDotNet.Common.Model.States
{
    public interface ITransitionMetadata : IModelMetadata
    {
        IEnumerable<string> Targets { get; }
        IEnumerable<string> Messages { get; }
        TransitionType Type { get; }
        TimeSpan? Delay { get; }

        bool EvalCondition(dynamic data);
        IEnumerable<IExecutableContentMetadata> GetExecutableContent();
    }
}
