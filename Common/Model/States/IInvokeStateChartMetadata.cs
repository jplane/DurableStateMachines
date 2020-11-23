using StateChartsDotNet.Common.Model.DataManipulation;
using StateChartsDotNet.Common.Model.Execution;
using System.Collections.Generic;

namespace StateChartsDotNet.Common.Model.States
{
    public interface IInvokeStateChartMetadata : IModelMetadata
    {
        bool Autoforward { get; }
        string Id { get; }
        string IdLocation { get; }

        IContentMetadata GetContent();
        IEnumerable<IExecutableContentMetadata> GetFinalizeExecutableContent();
        IEnumerable<IParamMetadata> GetParams();
     }
}
