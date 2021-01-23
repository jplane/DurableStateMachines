using Newtonsoft.Json;
using StateChartsDotNet.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace StateChartsDotNet.DurableFunction.Client
{
    internal class StateMachineResponsePayload
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("purgeHistoryDeleteUri")]
        public string PurgeHistoryDeleteUri { get; set; }

        [JsonProperty("sendEventPostUri")]
        public string SendEventPostUri { get; set; }

        [JsonProperty("statusQueryGetUri")]
        public string StatusQueryGetUri { get; set; }

        [JsonProperty("terminatePostUri")]
        public string TerminatePostUri { get; set; }

        public static StateMachineResponsePayload FromJson(string json)
        {
            json.CheckArgNull(nameof(json));
            return JsonConvert.DeserializeObject<StateMachineResponsePayload>(json);
        }
    }
}
