using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoreEngine.Abstractions.Model.Execution.Metadata
{
    public interface IIfMetadata : IExecutableContentMetadata
    {
        string IfConditionExpression { get; }

        IEnumerable<string> ElseIfConditionExpressions { get; }

        Task<IEnumerable<IExecutableContentMetadata>> GetExecutableContent();

        IEnumerable<Task<IEnumerable<IExecutableContentMetadata>>> GetElseIfExecutableContent();

        Task<IEnumerable<IExecutableContentMetadata>> GetElseExecutableContent();
    }
}
