﻿using StateChartsDotNet.Common.Model.DataManipulation;
using System;
using System.Collections.Generic;

namespace StateChartsDotNet.Common.Model.Execution
{
    public interface ISendMessageMetadata : IExecutableContentMetadata
    {
        string Id { get; }
        string IdLocation { get; }

        string GetType(dynamic data);
        TimeSpan GetDelay(dynamic data);
        string GetTarget(dynamic data);
        string GetMessageName(dynamic data);
        IContentMetadata GetContent();
        IEnumerable<IParamMetadata> GetParams();
    }
}