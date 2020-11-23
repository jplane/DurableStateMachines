using System.Collections.Generic;

namespace StateChartsDotNet.Common.Model
{
    public interface IModelMetadata
    {
        string UniqueId { get; }

        bool Validate(Dictionary<IModelMetadata, List<string>> errors);
    }
}
