using CoreEngine.Abstractions.Model.Execution.Metadata;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CoreEngine.Abstractions.Model.States.Metadata
{
    public interface ITransitionMetadata
    {
        IEnumerable<string> Targets { get; }
        IEnumerable<string> Events { get; }
        string ConditionExpr { get; }
        TransitionType Type { get; }

        Task<IEnumerable<IExecutableContentMetadata>> GetExecutableContent();
    }
}
