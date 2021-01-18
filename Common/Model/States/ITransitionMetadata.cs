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

    public static class TransitionMetadataExtensions
    {
        public static void Validate(this ITransitionMetadata metadata, Dictionary<IModelMetadata, List<string>> errors)
        {
            ((IModelMetadata) metadata).Validate(errors);

            if (! metadata.Targets.Any() && ! metadata.Messages.Any())
            {
                errors.Add(metadata, new List<string> { "Transition requires at least one target state or one target message." });
            }

            foreach (var executableContent in metadata.GetExecutableContent())
            {
                executableContent.Validate(errors);
            }
        }
    }
}
