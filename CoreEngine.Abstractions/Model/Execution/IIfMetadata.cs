using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.Abstractions.Model.Execution
{
    public interface IIfMetadata : IExecutableContentMetadata
    {
        bool EvalIfCondition(dynamic data);

        IEnumerable<Func<dynamic, bool>> GetElseIfConditions();

        IEnumerable<IExecutableContentMetadata> GetExecutableContent();

        IEnumerable<IEnumerable<IExecutableContentMetadata>> GetElseIfExecutableContent();

        IEnumerable<IExecutableContentMetadata> GetElseExecutableContent();
    }
}
