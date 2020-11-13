using StateChartsDotNet.CoreEngine.Abstractions.Model.DataManipulation;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.Abstractions.Model.States
{
    public interface IServiceMetadata
    {
        bool Autoforward { get; }
        string Type { get; }
        string TypeExpression { get; }
        string Id { get; }
        string IdLocation { get; }
        string Source { get; }
        string SourceExpression { get; }
        IEnumerable<string> Namelist { get; }

        Task<IContentMetadata> GetContent();
        Task<IFinalizeMetadata> GetFinalize();
        Task<IEnumerable<IParamMetadata>> GetParams();
     }
}
