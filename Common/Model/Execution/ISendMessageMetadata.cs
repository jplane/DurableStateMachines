using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace StateChartsDotNet.Common.Model.Execution
{
    public interface ISendMessageMetadata : IExecutableContentMetadata
    {
        string Id { get; }
        TimeSpan? Delay { get; }
        string ActivityType { get; }
        ISendMessageConfiguration Configuration { get; }
    }
}
