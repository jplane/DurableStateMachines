using System;
using System.Collections.Generic;
using System.Linq;

namespace DSM.Common.Model.Execution
{
    public interface IElseIfMetadata : IActionMetadata
    {
        bool EvalCondition(dynamic data);

        IEnumerable<IActionMetadata> GetActions();
    }
}
