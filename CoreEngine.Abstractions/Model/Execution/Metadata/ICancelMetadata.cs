using System;
using System.Collections.Generic;
using System.Text;

namespace CoreEngine.Abstractions.Model.Execution.Metadata
{
    public interface ICancelMetadata : IExecutableContentMetadata
    {
        string SendId { get; }
        string SendIdExpr { get; }
    }
}
