using System;
using System.Collections.Generic;
using System.Linq;

namespace DSM.Common.Model.Actions
{
    public interface IElseMetadata : IActionMetadata
    {
        IEnumerable<IActionMetadata> GetActions();
    }
}
