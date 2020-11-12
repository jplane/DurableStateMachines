using System;
using System.Threading.Tasks;

namespace CoreEngine.Abstractions.Model.DataManipulation.Metadata
{
    public interface IDataInitMetadata
    {
        string Id { get; }
        Task<object> GetValue(dynamic data);
    }
}
