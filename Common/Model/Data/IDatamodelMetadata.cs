using System.Collections.Generic;

namespace StateChartsDotNet.Common.Model.Data
{
    public interface IDatamodelMetadata : IModelMetadata
    {
        IEnumerable<IDataInitMetadata> GetData();
    }
}
