using System;
using System.Collections.Generic;
using System.Linq;

namespace StateChartsDotNet.Common.Model.Execution
{
    public interface IElseMetadata : IExecutableContentMetadata
    {
        IEnumerable<IExecutableContentMetadata> GetExecutableContent();
    }
}
