using System;
using System.Collections.Generic;
using System.Text;

namespace CoreEngine.Abstractions.Model.DataManipulation.Metadata
{
    public interface IContentMetadata
    {
        string Expression { get; }
        string Body { get; }
    }
}
