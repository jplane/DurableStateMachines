using StateChartsDotNet.CoreEngine.Abstractions.Model.DataManipulation;
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
        Task<IFinalizeMetadata> GetFinalize();
        Task<IEnumerable<IParamMetadata>> GetParams();
     }
}
