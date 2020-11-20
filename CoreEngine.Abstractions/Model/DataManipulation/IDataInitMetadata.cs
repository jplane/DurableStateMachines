using System;
using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.Abstractions.Model.DataManipulation
{
    public interface IDataInitMetadata : IModelMetadata
    {
        string Id { get; }
        object GetValue(dynamic data);
    }
}
