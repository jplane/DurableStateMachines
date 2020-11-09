using System;
using System.Collections.Generic;
using System.Text;

namespace CoreEngine.Abstractions.Model.DataManipulation.Metadata
{
    public interface IParamMetadata
    {
        string Name { get; }
        string Location { get; }
        string Expression { get; }
    }
}
