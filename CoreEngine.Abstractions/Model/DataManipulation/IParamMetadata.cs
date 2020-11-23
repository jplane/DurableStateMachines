using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.Abstractions.Model.DataManipulation
{
    public interface IParamMetadata : IModelMetadata
    {
        string Name { get; }
        object GetValue(dynamic data);
    }
}
