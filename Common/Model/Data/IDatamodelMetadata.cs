using System.Collections.Generic;

namespace StateChartsDotNet.Common.Model.Data
{
    public interface IDataModelMetadata : IModelMetadata
    {
        IEnumerable<IDataInitMetadata> GetData();
    }
}
