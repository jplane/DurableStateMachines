﻿using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common.Model.Execution;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace StateChartsDotNet.Common.Model.States
{
    public interface IStateChartMetadata : IStateMetadata
    {
        bool FailFast { get; }
        Databinding Databinding { get; }

        IEnumerable<IStateMetadata> GetStates();
        ITransitionMetadata GetInitialTransition();
        IScriptMetadata GetScript();

        (JObject, string) ToJson();
    }
}
