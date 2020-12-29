using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Data;
using StateChartsDotNet.Common.Model.Execution;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Metadata.Fluent.Data;
using StateChartsDotNet.Metadata.Fluent.Execution;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace StateChartsDotNet.Metadata.Fluent.States
{
    public sealed class InvokeStateChartMetadata<TParent> : IInvokeStateChartMetadata where TParent : IModelMetadata
    {
        private readonly List<ExecutableContentMetadata> _finalizeExecutableContent;
        private readonly List<ParamMetadata<InvokeStateChartMetadata<TParent>>> _params;

        private ChildStateChartExecutionMode _mode;
        private string _remoteUri;
        private string _id;
        private string _idLocation;
        private StateChart _root;

        internal InvokeStateChartMetadata()
        {
            _mode = ChildStateChartExecutionMode.Inline;
            _finalizeExecutableContent = new List<ExecutableContentMetadata>();
            _params = new List<ParamMetadata<InvokeStateChartMetadata<TParent>>>();
        }

        internal void Serialize(BinaryWriter writer)
        {
            writer.CheckArgNull(nameof(writer));

            writer.Write(this.MetadataId);
            writer.Write((int)_mode);
            writer.Write(_remoteUri);
            writer.Write(_id);
            writer.Write(_idLocation);

            writer.WriteMany(_finalizeExecutableContent, (o, w) => o.Serialize(w));
            writer.WriteMany(_params, (o, w) => o.Serialize(w));

            _root.Serialize(writer);
        }

        internal static InvokeStateChartMetadata<TParent> Deserialize(BinaryReader reader)
        {
            reader.CheckArgNull(nameof(reader));

            var metadata = new InvokeStateChartMetadata<TParent>();

            metadata.MetadataId = reader.ReadString();
            metadata._mode = (ChildStateChartExecutionMode)reader.ReadInt32();
            metadata._remoteUri = reader.ReadString();
            metadata._id = reader.ReadString();
            metadata._idLocation = reader.ReadString();

            metadata._finalizeExecutableContent.AddRange(ExecutableContentMetadata.DeserializeMany(reader, metadata));

            metadata._params.AddRange(reader.ReadMany(ParamMetadata<InvokeStateChartMetadata<TParent>>.Deserialize,
                                                       o => o.Parent = metadata));

            metadata._root = StateChart.Deserialize(reader);

            return metadata;
        }

        internal TParent Parent { get; set; }

        internal string MetadataId { private get; set; }

        public TParent Attach()
        {
            return this.Parent;
        }

        public InvokeStateChartMetadata<TParent> ExecutionMode(ChildStateChartExecutionMode mode)
        {
            _mode = mode;
            return this;
        }

        public InvokeStateChartMetadata<TParent> RemoteUri(string uri)
        {
            _remoteUri = uri;
            return this;
        }

        public InvokeStateChartMetadata<TParent> Id(string id)
        {
            _id = id;
            _idLocation = null;
            return this;
        }

        public InvokeStateChartMetadata<TParent> IdLocation(string idLocation)
        {
            _id = null;
            _idLocation = idLocation;
            return this;
        }

        public InvokeStateChartMetadata<TParent> Definition(StateChart statechart)
        {
            _root = statechart;
            return this;
        }

        public ParamMetadata<InvokeStateChartMetadata<TParent>> Param(string name)
        {
            var param = new ParamMetadata<InvokeStateChartMetadata<TParent>>(name);

            param.Parent = this;

            _params.Add(param);

            param.MetadataId = $"{((IModelMetadata)this).MetadataId}.Params[{_params.Count}]";

            return param;
        }

        public AssignMetadata<InvokeStateChartMetadata<TParent>> Assign()
        {
            var ec = new AssignMetadata<InvokeStateChartMetadata<TParent>>();

            _finalizeExecutableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.FinalizeExecutableContent[{_finalizeExecutableContent.Count}]";

            return ec;
        }

        public CancelMetadata<InvokeStateChartMetadata<TParent>> Cancel()
        {
            var ec = new CancelMetadata<InvokeStateChartMetadata<TParent>>();

            _finalizeExecutableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.FinalizeExecutableContent[{_finalizeExecutableContent.Count}]";

            return ec;
        }

        public ForeachMetadata<InvokeStateChartMetadata<TParent>> Foreach()
        {
            var ec = new ForeachMetadata<InvokeStateChartMetadata<TParent>>();

            _finalizeExecutableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.FinalizeExecutableContent[{_finalizeExecutableContent.Count}]";

            return ec;
        }

        public IfMetadata<InvokeStateChartMetadata<TParent>> If()
        {
            var ec = new IfMetadata<InvokeStateChartMetadata<TParent>>();

            _finalizeExecutableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.FinalizeExecutableContent[{_finalizeExecutableContent.Count}]";

            return ec;
        }

        public InvokeStateChartMetadata<TParent> Log(string message)
        {
            var ec = new LogMetadata<InvokeStateChartMetadata<TParent>>();

            ec.Message(message);

            _finalizeExecutableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.FinalizeExecutableContent[{_finalizeExecutableContent.Count}]";

            return this;
        }

        public InvokeStateChartMetadata<TParent> Log(Func<dynamic, string> getter)
        {
            var ec = new LogMetadata<InvokeStateChartMetadata<TParent>>();

            ec.Message(getter);

            _finalizeExecutableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.FinalizeExecutableContent[{_finalizeExecutableContent.Count}]";

            return this;
        }

        public InvokeStateChartMetadata<TParent> Raise(string messageName)
        {
            var ec = new RaiseMetadata<InvokeStateChartMetadata<TParent>>();

            ec.MessageName(messageName);

            _finalizeExecutableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.FinalizeExecutableContent[{_finalizeExecutableContent.Count}]";

            return this;
        }

        public InvokeStateChartMetadata<TParent> Execute(Action<dynamic> action)
        {
            var ec = new ScriptMetadata<InvokeStateChartMetadata<TParent>>();

            ec.Action(action);

            _finalizeExecutableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.FinalizeExecutableContent[{_finalizeExecutableContent.Count}]";

            return this;
        }

        internal SendMessageMetadata<InvokeStateChartMetadata<TParent>> SendMessage()
        {
            var ec = new SendMessageMetadata<InvokeStateChartMetadata<TParent>>();

            _finalizeExecutableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_finalizeExecutableContent.Count}]";

            return ec;
        }

        internal QueryMetadata<InvokeStateChartMetadata<TParent>> Query()
        {
            var ec = new QueryMetadata<InvokeStateChartMetadata<TParent>>();

            _finalizeExecutableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_finalizeExecutableContent.Count}]";

            return ec;
        }

        ChildStateChartExecutionMode IInvokeStateChartMetadata.ExecutionMode => _mode;

        string IInvokeStateChartMetadata.RemoteUri => _remoteUri;

        string IInvokeStateChartMetadata.Id => _id;

        string IInvokeStateChartMetadata.IdLocation => _idLocation;

        string IModelMetadata.MetadataId => this.MetadataId;

        IEnumerable<IExecutableContentMetadata> IInvokeStateChartMetadata.GetFinalizeExecutableContent() => _finalizeExecutableContent;

        IStateChartMetadata IInvokeStateChartMetadata.GetRoot() => _root;

        IReadOnlyDictionary<string, object> IInvokeStateChartMetadata.GetParams(dynamic data) =>
            new ReadOnlyDictionary<string, object>(_params.ToDictionary(p => p.Name, p => p.GetValue(data)));

        bool IModelMetadata.Validate(Dictionary<IModelMetadata, List<string>> errors)
        {
            throw new NotImplementedException();
        }
    }
}
