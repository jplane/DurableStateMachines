using StateChartsDotNet.CoreEngine.Abstractions.Model.DataManipulation;
using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.Abstractions.Model.States
{
    public interface IInvokeStateChart
    {
        bool Autoforward { get; }
        string Id { get; }
        string IdLocation { get; }

        Task<IContentMetadata> GetContent();
        Task<IEnumerable<IExecutableContentMetadata>> GetFinalizeExecutableContent();
        Task<IEnumerable<IParamMetadata>> GetParams();
     }
}
