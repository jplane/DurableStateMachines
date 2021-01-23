using Newtonsoft.Json;
using System.Linq;

namespace StateChartsDotNet.Common.Debugger
{
    public class DebuggerInfo
    {
        [JsonProperty("uri")]
        public string DebugUri { get; internal set; }

        [JsonProperty("instructions")]
        public DebuggerInstruction[] DebugInstructions { get; internal set; }

        internal bool IsMatch(DebuggerAction action, string element)
        {
            if (this.DebugInstructions == null)
            {
                return false;
            }
            else
            {
                return this.DebugInstructions.Any(di => di.IsMatch(action, element));
            }
        }
    }
}
