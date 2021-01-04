using System;
using System.Collections.Generic;
using System.Linq;

namespace StateChartsDotNet.Common.Model.Execution
{
    public interface IIfMetadata : IExecutableContentMetadata
    {
        bool EvalIfCondition(dynamic data);

        IEnumerable<Func<dynamic, bool>> GetElseIfConditions();

        IEnumerable<IExecutableContentMetadata> GetExecutableContent();

        IEnumerable<IEnumerable<IExecutableContentMetadata>> GetElseIfExecutableContent();

        IEnumerable<IExecutableContentMetadata> GetElseExecutableContent();
    }

    public static class IfMetadataExtensions
    {
        public static void Validate(this IIfMetadata metadata, Dictionary<IModelMetadata, List<string>> errors)
        {
            ((IModelMetadata) metadata).Validate(errors);

            foreach (var executableContent in metadata.GetExecutableContent())
            {
                executableContent.Validate(errors);
            }

            foreach (var executableContent in metadata.GetElseIfExecutableContent().SelectMany(ecs => ecs))
            {
                executableContent.Validate(errors);
            }

            foreach (var executableContent in metadata.GetElseExecutableContent())
            {
                executableContent.Validate(errors);
            }
        }
    }
}
