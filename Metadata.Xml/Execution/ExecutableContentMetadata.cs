using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
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

            switch (element.Name.LocalName)
            {
                case "if":
                    content = new IfMetadata(element);
                    break;

                case "raise":
                    content = new RaiseMetadata(element);
                    break;

                case "script":
                    content = new ScriptMetadata(element);
                    break;

                case "foreach":
                    content = new ForeachMetadata(element);
                    break;

                case "log":
                    content = new LogMetadata(element);
                    break;

                case "send":
                    content = new SendMessageMetadata(element);
                    break;

                case "cancel":
                    content = new CancelMetadata(element);
                    break;

                case "assign":
                    content = new AssignMetadata(element);
                    break;
            }

            Debug.Assert(content != null);

            return content;
        }
    }
}
