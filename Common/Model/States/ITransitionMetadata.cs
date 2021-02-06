using DSM.Common.Model.Actions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DSM.Common.Model.States
{
    public interface ITransitionMetadata : IModelMetadata
    {
        IEnumerable<string> Targets { get; }
        IEnumerable<string> Messages { get; }
        TransitionType Type { get; }
        TimeSpan? Delay { get; }

        bool EvalCondition(dynamic data);
        IEnumerable<IActionMetadata> GetActions();
    }
}
