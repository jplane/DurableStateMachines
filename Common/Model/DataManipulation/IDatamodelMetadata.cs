using System.Collections.Generic;

namespace StateChartsDotNet.Common.Model.DataManipulation
{
    public interface IDatamodelMetadata : IModelMetadata
    {
        IEnumerable<IDataInitMetadata> GetData();
    }
}
