using Newtonsoft.Json;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace StateChartsDotNet.Metadata.Execution
{
    public class Query : ExecutableContent, IQueryMetadata
    {
        private MetadataList<ExecutableContent> _actions;

        public Query()
        {
            this.Actions = new MetadataList<ExecutableContent>();
        }

        [JsonProperty("activitytype")]
        public string ActivityType { get; set; }

        [JsonProperty("resultlocation")]
        public string ResultLocation { get; set; }

        [JsonProperty("configuration")]
        public IQueryConfiguration Configuration { get; set; }

        [JsonProperty("actions", ItemConverterType = typeof(ExecutableContentConverter))]
        public MetadataList<ExecutableContent> Actions
        {
            get => _actions;

            set
            {
                if (value == null)
                {
                    throw new InvalidOperationException();
                }

                if (_actions != null)
                {
                    _actions.MetadataIdResolver = null;
                }

                value.MetadataIdResolver = () => $"{this.MetadataIdResolver?.Invoke(this) ?? "query"}.actions";

                _actions = value;
            }
        }

        internal override void Validate(IDictionary<string, List<string>> errorMap)
        {
            Debug.Assert(errorMap != null);

            var errors = new List<string>();

            if (this.Configuration == null)
            {
                errors.Add("Configuration is invalid.");
            }

            if (string.IsNullOrWhiteSpace(this.ActivityType))
            {
                errors.Add("ActivityType is invalid.");
            }

            if (string.IsNullOrWhiteSpace(this.ResultLocation))
            {
                errors.Add("ResultLocation is invalid.");
            }

            foreach (var action in this.Actions)
            {
                action.Validate(errorMap);
            }

            if (errors.Any())
            {
                errorMap.Add(((IModelMetadata)this).MetadataId, errors);
            }
        }

        IEnumerable<IExecutableContentMetadata> IQueryMetadata.GetExecutableContent() =>
            this.Actions ?? Enumerable.Empty<IExecutableContentMetadata>();
    }
}
