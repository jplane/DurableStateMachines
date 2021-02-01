using System;
using System.Collections.Generic;
using System.Linq;

namespace DSM.Common.Model.Execution
{
    public interface IElseMetadata : IExecutableContentMetadata
    {
        IEnumerable<IExecutableContentMetadata> GetExecutableContent();
    }
}
