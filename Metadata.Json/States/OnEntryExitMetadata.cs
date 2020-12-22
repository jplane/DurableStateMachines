using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Metadata.Json.Execution;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace StateChartsDotNet.Metadata.Json.States
{
    public class OnEntryExitMetadata : IOnEntryExitMetadata
    {
        private readonly JObject _element;
        private readonly string _metadataId;

        internal OnEntryExitMetadata(JObject element)
        {
            _element = element;
            _metadataId = element.GetUniqueElementPath();
        }

        public string MetadataId => _metadataId;

        public virtual bool Validate(Dictionary<IModelMetadata, List<string>> errors)
        {
            return true;
        }

        public bool IsEntry
        {
            get
            {
                var parent = _element.Parent;

                Debug.Assert(parent != null);
                Debug.Assert(parent is JProperty);

                return ((JProperty) parent).Name.ToLowerInvariant() == "onentry";
            }
        }

        public IEnumerable<IExecutableContentMetadata> GetExecutableContent()
        {
            var content = new List<IExecutableContentMetadata>();

            var elements = _element.Property("content");

            if (elements != null)
            {
                foreach (var node in elements.Value.Values<JObject>())
                {
                    content.Add(ExecutableContentMetadata.Create(node));
                }
            }

            return content;
        }
    }
}
