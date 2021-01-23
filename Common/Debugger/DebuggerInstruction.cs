using Newtonsoft.Json;
using System.Globalization;

namespace StateChartsDotNet.Common.Debugger
{
    public class DebuggerInstruction
    {
        [JsonProperty("action")]
        public DebuggerAction Action { get; internal set; }

        [JsonProperty("element")]
        public string Element { get; internal set; }

        internal bool IsMatch(DebuggerAction action, string element)
        {
            if (action != this.Action)
            {
                return false;
            }
            else if (this.Element == "*")
            {
                return true;
            }
            else if (string.Compare(element,
                                    this.Element,
                                    true,
                                    CultureInfo.InvariantCulture) == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
