using System;
using System.Collections.Generic;
using System.Text;

namespace CoreEngine.Abstractions.Model.Execution.Metadata
{
    public interface ILogMetadata : IExecutableContentMetadata
    {
        string Message { get; }
    }
}
