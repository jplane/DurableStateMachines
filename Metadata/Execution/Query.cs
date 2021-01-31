using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace StateChartsDotNet.Metadata.Execution
{
    public class Query<TData> : ExecutableContent<TData>, IQueryMetadata
    {
        private MemberInfo _resultTarget;
        private MetadataList<ExecutableContent<TData>> _actions;

        public Query()
        {
            this.Actions = new MetadataList<ExecutableContent<TData>>();
        }

        [JsonProperty("activitytype")]
        public string ActivityType { get; set; }

        public Expression<Func<TData, object>> ResultTarget
        {
            set => _resultTarget = value.ExtractMember(nameof(ResultTarget));
        }

        [JsonProperty("resultlocation")]
        private string ResultLocation { get; set; }

        [JsonProperty("configuration")]
        public IQueryConfiguration Configuration { get; set; }

        [JsonProperty("actions")]
        public MetadataList<ExecutableContent<TData>> Actions
        {
            get => _actions;

            private set
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

            if (string.IsNullOrWhiteSpace(this.ResultLocation) && _resultTarget == null)
            {
                errors.Add("Result target/location is invalid.");
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

        (string, MemberInfo) IQueryMetadata.ResultLocation => (this.ResultLocation, _resultTarget);

        IEnumerable<IExecutableContentMetadata> IQueryMetadata.GetExecutableContent() =>
            this.Actions ?? Enumerable.Empty<IExecutableContentMetadata>();
    }
}
