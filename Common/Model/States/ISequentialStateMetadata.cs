using System.Collections.Generic;

namespace StateChartsDotNet.Common.Model.States
{
    public interface ISequentialStateMetadata : IStateMetadata
    {
        ITransitionMetadata GetInitialTransition();
        IEnumerable<IStateMetadata> GetStates();
    }

    public static class SequentialStateMetadataExtensions
    {
        public static void Validate(this ISequentialStateMetadata metadata, Dictionary<IModelMetadata, List<string>> errors)
        {
            ((IStateMetadata) metadata).Validate(errors);

            var transition = metadata.GetInitialTransition();

            if (transition == null)
            {
                errors.Add(metadata, new List<string> { "Sequential state requires an initial transition." });
            }
            else
            {
                transition.Validate(errors);
            }

            foreach (var state in metadata.GetStates())
            {
                state.Validate(errors);
            }
        }
    }
}
