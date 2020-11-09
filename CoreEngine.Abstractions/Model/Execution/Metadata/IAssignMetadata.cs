using System;
using System.Collections.Generic;
using System.Text;

namespace CoreEngine.Abstractions.Model.Execution.Metadata
{
    public interface IAssignMetadata : IExecutableContentMetadata
    {
        string Location { get; }
        string Expression { get; }
        string Body { get; }
    }
}
