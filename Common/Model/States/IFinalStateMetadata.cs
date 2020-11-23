using System;
using System.Collections.Generic;

namespace StateChartsDotNet.Common.Model.States
{
    public interface IFinalStateMetadata : IStateMetadata
    {
        object GetContent(dynamic data);
        IReadOnlyDictionary<string, object> GetParams(dynamic data);
    }
}
