using System;
using System.Collections.Generic;
using System.Linq;

namespace DSM.Common.Model.Execution
{
    public interface IIfMetadata : IExecutableContentMetadata
    {
        bool EvalCondition(dynamic data);

        IEnumerable<IExecutableContentMetadata> GetExecutableContent();

        IEnumerable<IElseIfMetadata> GetElseIfs();

        IElseMetadata GetElse();
    }
}
