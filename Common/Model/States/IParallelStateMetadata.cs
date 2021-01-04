using System.Collections.Generic;

namespace StateChartsDotNet.Common.Model.States
{
    public interface IParallelStateMetadata : IStateMetadata
    {
        IEnumerable<IStateMetadata> GetStates();
    }

    public static class ParallelStateMetadataExtensions
    {
        public static void Validate(this IParallelStateMetadata metadata, Dictionary<IModelMetadata, List<string>> errors)
        {
            ((IStateMetadata) metadata).Validate(errors);

            foreach (var state in metadata.GetStates())
            {
                state.Validate(errors);
            }
        }
    }
}
