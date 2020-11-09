using CoreEngine.Abstractions.Model.DataManipulation.Metadata;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CoreEngine.Abstractions.Model.Execution.Metadata
{
    public interface ISendMetadata : IExecutableContentMetadata
    {
        string Event { get; }
        string EventExpression { get; }
        string Target { get; }
        string TargetExpression { get; }
        string Type { get; }
        string TypeExpression { get; }
        string Id { get; }
        string IdLocation { get; }
        string Delay { get; }
        string DelayExpression { get; }
        IEnumerable<string> Namelist { get; }

        Task<IContentMetadata> GetContent();
        Task<IEnumerable<IParamMetadata>> GetParams();
    }
}
