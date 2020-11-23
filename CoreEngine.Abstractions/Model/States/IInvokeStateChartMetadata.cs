using StateChartsDotNet.CoreEngine.Abstractions.Model.DataManipulation;
using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.Abstractions.Model.States
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
