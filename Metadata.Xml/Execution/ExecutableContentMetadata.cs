using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using StateChartsDotNet.Metadata.Xml.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;

namespace StateChartsDotNet.Metadata.Xml.Execution
{
    public abstract class ExecutableContentMetadata : IExecutableContentMetadata
    {
        private readonly Lazy<string> _uniqueId;
        protected readonly XElement _element;

        protected ExecutableContentMetadata(XElement element)
        {
            _element = element;

            _uniqueId = new Lazy<string>(() =>
            {
                return element.GetUniqueElementPath();
            });
        }

        public string UniqueId => _uniqueId.Value;

        public virtual bool Validate(Dictionary<IModelMetadata, List<string>> errors)
        {
            return true;
        }

        public static IExecutableContentMetadata Create(XElement element)
        {
            element.CheckArgNull(nameof(element));

            IExecutableContentMetadata content = null;

            content = element.Name.LocalName switch
            {
                "if" => new IfMetadata(element),
                "raise" => new RaiseMetadata(element),
                "script" => new ScriptMetadata(element),
                "foreach" => new ForeachMetadata(element),
                "log" => new LogMetadata(element),
                "cancel" => new CancelMetadata(element),
                "assign" => new AssignMetadata(element),
                _ => StateChart.ServiceResolver(element),
            };

            Debug.Assert(content != null);

            return content;
        }
    }
}
