using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreEngine.Abstractions.Model.Execution.Metadata
{
    public interface IIfMetadata : IExecutableContentMetadata
    {
        Task<bool> EvalIfCondition(dynamic data);

        Task<IEnumerable<Func<dynamic, Task<bool>>>> GetElseIfConditions();

        Task<IEnumerable<IExecutableContentMetadata>> GetExecutableContent();

        IEnumerable<Task<IEnumerable<IExecutableContentMetadata>>> GetElseIfExecutableContent();

        Task<IEnumerable<IExecutableContentMetadata>> GetElseExecutableContent();
    }
}
