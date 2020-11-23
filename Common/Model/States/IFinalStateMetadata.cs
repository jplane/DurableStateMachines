using System;
using System.Collections.Generic;

namespace StateChartsDotNet.Common.Model.States
{
    public interface IFinalStateMetadata : IStateMetadata
    {
        object GetContent(dynamic data);
        IReadOnlyDictionary<string, Func<dynamic, object>> GetParams();
    }
}
