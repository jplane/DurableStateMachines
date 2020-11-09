using System;
using System.Collections.Generic;
using System.Text;

namespace CoreEngine.Abstractions.Model.Execution.Metadata
{
    public interface IRaiseMetadata : IExecutableContentMetadata
    {
        string Event { get; }
    }
}
