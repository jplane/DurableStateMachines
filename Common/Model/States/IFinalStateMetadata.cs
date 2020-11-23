using StateChartsDotNet.Common.Model.DataManipulation;
using System.Collections.Generic;

namespace StateChartsDotNet.Common.Model.States
{
    public interface IFinalStateMetadata : IStateMetadata
    {
        IContentMetadata GetContent();
        IEnumerable<IParamMetadata> GetParams();
    }
}
