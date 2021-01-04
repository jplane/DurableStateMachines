using StateChartsDotNet.Common.Model.Data;
using System.Collections.Generic;

namespace StateChartsDotNet.Common.Model.States
{
    public interface IStateMetadata : IModelMetadata
    {
        string Id { get; }

        bool IsDescendentOf(IStateMetadata state);

        int DepthFirstCompare(IStateMetadata metadata);

        IOnEntryExitMetadata GetOnEntry();

        IOnEntryExitMetadata GetOnExit();

        IEnumerable<ITransitionMetadata> GetTransitions();

        IEnumerable<IInvokeStateChartMetadata> GetStateChartInvokes();

        IDatamodelMetadata GetDatamodel();
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

            metadata.GetOnEntry()?.Validate(errors);

            metadata.GetOnExit()?.Validate(errors);

            metadata.GetDatamodel()?.Validate(errors);

            foreach (var transition in metadata.GetTransitions())
            {
                transition.Validate(errors);
            }

            foreach (var invoke in metadata.GetStateChartInvokes())
            {
                invoke.Validate(errors);
            }
        }
    }
}
