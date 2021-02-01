using Newtonsoft.Json;
using System.Linq;

namespace StateChartsDotNet.Common.Debugger
{
    public class DebuggerInfo
    {
        [JsonProperty("instructions")]
        public DebuggerInstruction[] Instructions { get; set; }

        internal bool IsMatch(DebuggerAction action, string element)
        {
            if (this.Instructions == null)
            {
                return false;
            }
            else
            {
                return this.Instructions.Any(di => di.IsMatch(action, element));
            }
        }
    }
}
