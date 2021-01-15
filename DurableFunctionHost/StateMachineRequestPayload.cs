﻿using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Metadata.Json.States;
using System;
using System.Collections.Generic;

namespace DurableFunctionHost
{
    public class StateMachineRequestPayload
    {
        [JsonProperty("args")]
        public Dictionary<string, object> Arguments { get; internal set; }

        [JsonProperty("statemachine")]
        public JObject StateMachineDefinition { get; internal set; }

        internal IStateChartMetadata GetMetadata()
        {
            if (this.StateMachineDefinition == null)
            {
                return null;
            }

            return new StateChart(this.StateMachineDefinition);
        }
    }
}