using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.DataManipulation;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Metadata.Xml.DataManipulation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;

namespace StateChartsDotNet.Metadata.Xml.States
{
    public class FinalStateMetadata : StateMetadata, IFinalStateMetadata
    {
        private readonly Lazy<Func<dynamic, object>> _getContentValue;

        public FinalStateMetadata(XElement element)
            : base(element)
        {
            _getContentValue = new Lazy<Func<dynamic, object>>(() =>
            {
                var node = _element.ScxmlElement("donedata").ScxmlElement("content");

                if (node == null)
                {
                    return _ => string.Empty;
                }

                var expression = node.Attribute("expr")?.Value;

                if (!string.IsNullOrWhiteSpace(expression))
                {
                    return ExpressionCompiler.Compile<object>(expression);
                }
                else
                {
                    return _ => node.Value ?? string.Empty;
                }
            });
        }

        public object GetContent(dynamic data)
        {
            return _getContentValue.Value(data);
        }

        public IReadOnlyDictionary<string, Func<dynamic, object>> GetParams()
        {
            var nodes = _element.ScxmlElement("donedata").ScxmlElements("param");

            return new ReadOnlyDictionary<string, Func<dynamic, object>>(
                nodes.Select(n => new ParamMetadata(n)).ToDictionary(p => p.Name, p => (Func<dynamic, object>)p.GetValue));
        }
    }
}
