using System.Collections.Generic;

namespace StateChartsDotNet.Common.Model
{
    public interface IModelMetadata
    {
        string MetadataId { get; }

        bool Validate(Dictionary<IModelMetadata, List<string>> errors);
    }
}
