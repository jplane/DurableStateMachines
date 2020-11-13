using System;
using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.Abstractions.Model.DataManipulation
{
    public interface IDataInitMetadata
    {
        string Id { get; }
        Task<object> GetValue(dynamic data);
    }
}
