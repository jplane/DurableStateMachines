using Newtonsoft.Json;
using System.Linq;

namespace DSM.Common.Debugger
{
    public sealed class DebuggerInfo
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
