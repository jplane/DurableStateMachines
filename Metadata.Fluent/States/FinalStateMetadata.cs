using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Data;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Metadata.Fluent.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace StateChartsDotNet.Metadata.Fluent.States
{
    public sealed class FinalStateMetadata<TParent> : StateMetadata, IFinalStateMetadata where TParent : IStateMetadata
    {
        private object _content;
        private Func<dynamic, object> _contentGetter;
        private OnEntryExitMetadata<FinalStateMetadata<TParent>> _onEntry;
        private OnEntryExitMetadata<FinalStateMetadata<TParent>> _onExit;

        private readonly List<ParamMetadata<FinalStateMetadata<TParent>>> _params =
            new List<ParamMetadata<FinalStateMetadata<TParent>>>();

        internal FinalStateMetadata(string id)
            : base(id)
        {
        }

        internal override void Serialize(BinaryWriter writer)
        {
            writer.CheckArgNull(nameof(writer));

            base.Serialize(writer);

            writer.WriteObject(_content);
            writer.Write(_contentGetter);

            writer.Write(_onEntry, (o, w) => o.Serialize(w));
            writer.Write(_onExit, (o, w) => o.Serialize(w));

            writer.WriteMany(_params, (o, w) => o.Serialize(w));
        }

        internal static FinalStateMetadata<TParent> Deserialize(BinaryReader reader)
        {
            reader.CheckArgNull(nameof(reader));

            var id = reader.ReadString();

            var metadata = new FinalStateMetadata<TParent>(id);

            metadata.MetadataId = reader.ReadString();

            metadata._content = reader.ReadObject();

            metadata._contentGetter = reader.Read<Func<dynamic, object>>();

            metadata._onEntry = reader.Read(OnEntryExitMetadata<FinalStateMetadata<TParent>>.Deserialize,
                                o => o.Parent = metadata);

            metadata._onExit = reader.Read(OnEntryExitMetadata<FinalStateMetadata<TParent>>.Deserialize,
                                           o => o.Parent = metadata);

            metadata._params.AddRange(reader.ReadMany(ParamMetadata<FinalStateMetadata<TParent>>.Deserialize,
                                                       o => o.Parent = metadata));

            return metadata;
        }

        protected override IStateMetadata _Parent => this.Parent;

        internal TParent Parent { get; set; }

        protected override IOnEntryExitMetadata GetOnEntry() => _onEntry;

        public TParent Attach()
        {
            return this.Parent;
        }

        public OnEntryExitMetadata<FinalStateMetadata<TParent>> OnEntry()
        {
            _onEntry = new OnEntryExitMetadata<FinalStateMetadata<TParent>>(true);

            _onEntry.Parent = this;

            _onEntry.MetadataId = $"{((IModelMetadata)this).MetadataId}.OnEntry";

            return _onEntry;
        }

        protected override IOnEntryExitMetadata GetOnExit() => _onExit;

        public OnEntryExitMetadata<FinalStateMetadata<TParent>> OnExit()
        {
            _onExit = new OnEntryExitMetadata<FinalStateMetadata<TParent>>(false);

            _onExit.Parent = this;

            _onExit.MetadataId = $"{((IModelMetadata)this).MetadataId}.OnExit";

            return _onExit;
        }

        public FinalStateMetadata<TParent> Content(object content)
        {
            _content = content;
            _contentGetter = null;
            return this;
        }

        public FinalStateMetadata<TParent> Content(Func<dynamic, object> getter)
        {
            _contentGetter = getter;
            _content = null;
            return this;
        }

        public ParamMetadata<FinalStateMetadata<TParent>> Param(string name)
        {
            var param = new ParamMetadata<FinalStateMetadata<TParent>>(name);

            param.Parent = this;

            _params.Add(param);

            param.MetadataId = $"{((IModelMetadata)this).MetadataId}.Params[{_params.Count}]";

            return param;
        }

        object IFinalStateMetadata.GetContent(dynamic data) =>
            _contentGetter == null ? _content : _contentGetter.Invoke(data);

        IReadOnlyDictionary<string, object> IFinalStateMetadata.GetParams(dynamic data) =>
            new ReadOnlyDictionary<string, object>(_params.ToDictionary(p => p.Name, p => p.GetValue(data)));
    }
}
