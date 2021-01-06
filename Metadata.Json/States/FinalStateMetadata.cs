using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Data;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Metadata.Json.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;


namespace StateChartsDotNet.Metadata.Json.States
{
    public class FinalStateMetadata : StateMetadata, IFinalStateMetadata
    {
        private readonly Lazy<Func<dynamic, object>> _getContentValue;

        internal FinalStateMetadata(JObject element)
            : base(element)
        {
            _getContentValue = new Lazy<Func<dynamic, object>>(() =>
            {
                var node = _element.Property("donedata")?.Value.Value<JObject>()?.Property("content");

                if (node == null)
                {
                    return _ => string.Empty;
                }
                else if (node.Value is JObject)
                {
                    return _ => node.Value.ToString();
                }
                else
                {
                    var expression = node.Value.Value<string>();

                    return ExpressionCompiler.Compile<object>(expression);
                }
            });
        }

        public override StateType Type => StateType.Final;

        public object GetContent(dynamic data)
        {
            return _getContentValue.Value(data);
        }

        public IReadOnlyDictionary<string, object> GetParams(dynamic data)
        {
            var node = _element.Property("donedata")?.Value.Value<JObject>()?.Property("params");

            var nodes = node?.Value.Values<JObject>() ?? Enumerable.Empty<JObject>();

            return new ReadOnlyDictionary<string, object>(
                nodes.Select(n => new ParamMetadata(n)).ToDictionary(p => p.Name, p => p.GetValue(data)));
        }
    }
}
