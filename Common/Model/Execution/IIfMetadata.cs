using System;
using System.Collections.Generic;

namespace StateChartsDotNet.Common.Model.Execution
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
