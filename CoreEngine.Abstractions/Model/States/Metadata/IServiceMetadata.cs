using CoreEngine.Abstractions.Model.DataManipulation.Metadata;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreEngine.Abstractions.Model.States.Metadata
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
