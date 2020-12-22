using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Exceptions;
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
        private readonly string _metadataId;
        protected readonly XElement _element;

        protected ExecutableContentMetadata(XElement element)
        {
            _element = element;
            _metadataId = element.GetUniqueElementPath();
        }

        public string MetadataId => _metadataId;

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
                throw new MetadataValidationException("Unable to resolve executable content type: " + element.Name.LocalName);
            }

            return content;
        }
    }
}
