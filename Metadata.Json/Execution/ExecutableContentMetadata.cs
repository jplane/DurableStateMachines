using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.Execution;
using System;
using System.Collections.Generic;

namespace StateChartsDotNet.Metadata.Json.Execution
{
    public abstract class ExecutableContentMetadata : IExecutableContentMetadata
    {
        private readonly string _metadataId;
        protected readonly JObject _element;

        protected ExecutableContentMetadata(JObject element)
        {
            _element = element;
            _metadataId = element.GetUniqueElementPath();
        }

        public string MetadataId => _metadataId;

        public IReadOnlyDictionary<string, object> DebuggerInfo => null;

        public static IExecutableContentMetadata Create(JObject element)
        {
            element.CheckArgNull(nameof(element));

            var type = element.Property("type").Value.Value<string>();

            var content = type switch
            {
                "if" => new IfMetadata(element),
                "raise" => new RaiseMetadata(element),
                "script" => new ScriptMetadata(element),
                "foreach" => new ForeachMetadata(element),
                "log" => new LogMetadata(element),
                "cancel" => new CancelMetadata(element),
                "assign" => new AssignMetadata(element),
                "query" => new QueryMetadata(element),
                "sendmessage" => new SendMessageMetadata(element),
                _ => (IExecutableContentMetadata) null
            };

            if (content == null)
            {
                throw new InvalidOperationException("Unable to resolve executable content type: " + type);
            }

            return content;
        }
    }
}
