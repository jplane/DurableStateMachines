using CoreEngine.Abstractions.Model.DataManipulation.Metadata;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CoreEngine.Abstractions.Model.States.Metadata
{
    public interface IStateMetadata
    {
        string Id { get; }

        bool IsDescendentOf(IStateMetadata state);

        int DepthFirstCompare(IStateMetadata metadata);

        Task<IOnEntryExitMetadata> GetOnEntry();

        Task<IOnEntryExitMetadata> GetOnExit();

        Task<IEnumerable<ITransitionMetadata>> GetTransitions();

        Task<IEnumerable<IServiceMetadata>> GetServices();

        Task<IDatamodelMetadata> GetDatamodel();
    }
}
