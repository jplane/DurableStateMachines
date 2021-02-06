using System;
using System.Collections.Generic;
using System.Linq;

namespace DSM.Common.Model.Actions
{
    public interface IElseIfMetadata : IActionMetadata
    {
        bool EvalCondition(dynamic data);

        IEnumerable<IActionMetadata> GetActions();
    }
}
