using System;
using System.Collections.Generic;
using System.Linq;

namespace DSM.Common.Model.Actions
{
    public interface IIfMetadata : IActionMetadata
    {
        bool EvalCondition(dynamic data);

        IEnumerable<IActionMetadata> GetActions();

        IEnumerable<IElseIfMetadata> GetElseIfs();

        IElseMetadata GetElse();
    }
}
