using StateChartsDotNet.CoreEngine.Abstractions.Model.States;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.Abstractions.Model
{
    public interface IModelMetadata
    {
        string UniqueId { get; }

        bool Validate(Dictionary<IModelMetadata, List<string>> errors);
    }
}
