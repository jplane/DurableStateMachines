using System.Collections.Generic;

namespace StateChartsDotNet.Common.Model.Data
{
    public interface IDataInitMetadata : IModelMetadata
    {
        string Id { get; }
        object GetValue(dynamic data);
    }
}
