using System;
using System.Collections.Generic;
using System.Text;

namespace CoreEngine.Abstractions.Model.States.Metadata
{
    public interface IHistoryStateMetadata : IStateMetadata
    {
        HistoryType Type { get; }
    }
}
