using CoreEngine.Abstractions.Model.Execution.Metadata;
using System.Diagnostics;
using System.Xml.Linq;

namespace CoreEngine.ModelProvider.Xml.Execution
{
    public abstract class ExecutableContentMetadata : IExecutableContentMetadata
    {
        protected readonly XElement _element;

        protected ExecutableContentMetadata(XElement element)
        {
            _element = element;
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
                    content = new SendMetadata(element);
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
