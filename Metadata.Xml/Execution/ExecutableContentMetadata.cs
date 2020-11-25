using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using StateChartsDotNet.Metadata.Xml.Queries;
using StateChartsDotNet.Metadata.Xml.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

            var resolvers = new Func<XElement, IExecutableContentMetadata>[]
            {
                ServiceResolver.Resolve,
                QueryResolver.Resolve 
            };

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
                _ => resolvers.Select(func => func(element)).FirstOrDefault(result => result != null)
            };

            if (content == null)
            {
                throw new NotSupportedException("Unable to resolve executable content type: " + element.Name.LocalName);
            }

            return content;
        }
    }
}
