using CoreEngine.Abstractions.Model.Execution.Metadata;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreEngine.Abstractions.Model.States.Metadata
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
