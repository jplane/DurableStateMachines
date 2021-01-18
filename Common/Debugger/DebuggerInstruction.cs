using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace StateChartsDotNet.Common.Debugger
{
    public class DebuggerInstruction
    {
        [JsonProperty("action", ItemConverterType = typeof(StringEnumConverter))]
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
