using StateChartsDotNet.Common.Model.Data;
using System.Collections.Generic;
using System.Linq;

namespace StateChartsDotNet.Common.Model.States
{
    public interface IStateMetadata : IModelMetadata
    {
        string Id { get; }

        StateType Type { get; }

        bool IsDescendentOf(IStateMetadata state);

        int DepthFirstCompare(IStateMetadata metadata);

        IOnEntryExitMetadata GetOnEntry();

        IOnEntryExitMetadata GetOnExit();

        IEnumerable<ITransitionMetadata> GetTransitions();

        IEnumerable<IInvokeStateChartMetadata> GetStateChartInvokes();

        IDatamodelMetadata GetDatamodel();

        ITransitionMetadata GetInitialTransition();

        IEnumerable<IStateMetadata> GetStates();
    }

    public static class StateMetadataExtensions
    {
        public static void Validate(this IStateMetadata metadata, Dictionary<IModelMetadata, List<string>> errors)
        {
            ((IModelMetadata) metadata).Validate(errors);

            if (string.IsNullOrWhiteSpace(metadata.Id))
            {
                errors.Add(metadata, new List<string> { "Id is invalid." });
            }

            var initialTransition = metadata.GetInitialTransition();

            var states = metadata.GetStates().ToArray();

            if ((initialTransition == null && states.Length > 0) ||
                (initialTransition != null && states.Length == 0))
            {
                errors.Add(metadata, new List<string> { "Compound state requires an initial transition and at least one child state." });
            }
            else
            {
                initialTransition?.Validate(errors);
            }

            foreach (var state in states)
            {
                state.Validate(errors);
            }

            metadata.GetOnEntry()?.Validate(errors);

            metadata.GetOnExit()?.Validate(errors);

            metadata.GetDatamodel()?.Validate(errors);

            foreach (var outboundTransition in metadata.GetTransitions())
            {
                outboundTransition.Validate(errors);
            }

            foreach (var invoke in metadata.GetStateChartInvokes())
            {
                invoke.Validate(errors);
            }
        }
    }
}
