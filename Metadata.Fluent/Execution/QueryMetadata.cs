using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;

namespace StateChartsDotNet.Metadata.Fluent.Execution
{
    public sealed class QueryMetadata<TParent> : ExecutableContentMetadata, IQueryMetadata where TParent : IModelMetadata
    {
        private readonly List<ExecutableContentMetadata> _executableContent;
        private IQueryConfiguration _config;
        private string _resultLocation;
        private string _activityType;

        internal QueryMetadata()
        {
            _executableContent = new List<ExecutableContentMetadata>();
        }

        internal override void Serialize(BinaryWriter writer)
        {
            writer.CheckArgNull(nameof(writer));

            base.Serialize(writer);

            writer.WriteNullableString(_resultLocation);
            writer.WriteNullableString(_activityType);
            writer.WriteMany(_executableContent, (o, w) => o.Serialize(w));
            writer.WriteObject(_config);
        }

        internal static QueryMetadata<TParent> Deserialize(BinaryReader reader)
        {
            reader.CheckArgNull(nameof(reader));

            var metadata = new QueryMetadata<TParent>();

            metadata.MetadataId = reader.ReadNullableString();
            metadata._resultLocation = reader.ReadNullableString();
            metadata._activityType = reader.ReadNullableString();

            metadata._executableContent.AddRange(reader.ReadMany(ExecutableContentMetadata._Deserialize,
                                                    o => ((dynamic)o).Parent = metadata));

            metadata._config = (IQueryConfiguration) reader.ReadObject();

            return metadata;
        }

        internal TParent Parent { get; set; }

        public TParent _ => this.Parent;

        public QueryMetadata<TParent> ResultLocation(string resultLocation)
        {
            _resultLocation = resultLocation;
            return this;
        }

        public QueryMetadata<TParent> ActivityType(string activityType)
        {
            _activityType = activityType;
            return this;
        }

        internal IQueryConfiguration Configuration
        {
            get => _config;
            set => _config = value;
        }

        public QueryMetadata<TParent> Assign(string location, object value)
        {
            var ec = new AssignMetadata<QueryMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.FinalizeExecutableContent[{_executableContent.Count}]";

            ec.Location(location).Value(value);

            return this;
        }

        public QueryMetadata<TParent> Assign(string location, Expression<Func<IDictionary<string, object>, object>> getter)
        {
            var ec = new AssignMetadata<QueryMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.FinalizeExecutableContent[{_executableContent.Count}]";

            ec.Location(location).Value(getter);

            return this;
        }

        public CancelMetadata<QueryMetadata<TParent>> Cancel
        {
            get
            {
                var ec = new CancelMetadata<QueryMetadata<TParent>>();

                _executableContent.Add(ec);

                ec.Parent = this;

                ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.FinalizeExecutableContent[{_executableContent.Count}]";

                return ec;
            }
        }

        public ForeachMetadata<QueryMetadata<TParent>> Foreach
        {
            get
            {
                var ec = new ForeachMetadata<QueryMetadata<TParent>>();

                _executableContent.Add(ec);

                ec.Parent = this;

                ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.FinalizeExecutableContent[{_executableContent.Count}]";

                return ec;
            }
        }

        public IfMetadata<QueryMetadata<TParent>> If
        {
            get
            {
                var ec = new IfMetadata<QueryMetadata<TParent>>();

                _executableContent.Add(ec);

                ec.Parent = this;

                ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.FinalizeExecutableContent[{_executableContent.Count}]";

                return ec;
            }
        }

        public QueryMetadata<TParent> Log(string message)
        {
            var ec = new LogMetadata<QueryMetadata<TParent>>();

            ec.Message(message);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.FinalizeExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public QueryMetadata<TParent> Log(Expression<Func<IDictionary<string, object>, string>> getter)
        {
            var ec = new LogMetadata<QueryMetadata<TParent>>();

            ec.Message(getter);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.FinalizeExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public QueryMetadata<TParent> Raise(string messageName)
        {
            var ec = new RaiseMetadata<QueryMetadata<TParent>>();

            ec.MessageName(messageName);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.FinalizeExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public QueryMetadata<TParent> Execute(Expression<Action<IDictionary<string, object>>> action)
        {
            var ec = new ScriptMetadata<QueryMetadata<TParent>>();

            ec.Action(action);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.FinalizeExecutableContent[{_executableContent.Count}]";

            return this;
        }

        internal SendMessageMetadata<QueryMetadata<TParent>> SendMessage
        {
            get
            {
                var ec = new SendMessageMetadata<QueryMetadata<TParent>>();

                _executableContent.Add(ec);

                ec.Parent = this;

                ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

                return ec;
            }
        }

        internal QueryMetadata<QueryMetadata<TParent>> Query
        {
            get
            {
                var ec = new QueryMetadata<QueryMetadata<TParent>>();

                _executableContent.Add(ec);

                ec.Parent = this;

                ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

                return ec;
            }
        }

        string IQueryMetadata.ResultLocation => _resultLocation;

        string IQueryMetadata.ActivityType => _activityType;

        IQueryConfiguration IQueryMetadata.Configuration => _config;

        IEnumerable<IExecutableContentMetadata> IQueryMetadata.GetExecutableContent() => _executableContent;
    }
}
