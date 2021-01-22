using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.States;
using System.Collections.Generic;
using System.IO;

namespace StateChartsDotNet.Metadata.Fluent.States
{
    public sealed class HistoryStateMetadata<TParent> : StateMetadata, IHistoryStateMetadata where TParent : IStateMetadata
    {
        private TransitionMetadata<HistoryStateMetadata<TParent>> _transition;
        private bool _isDeep;

        internal HistoryStateMetadata(string id)
            : base(id)
        {
            _isDeep = false;
        }

        internal override void Serialize(BinaryWriter writer)
        {
            writer.CheckArgNull(nameof(writer));

            base.Serialize(writer);

            writer.Write(_isDeep);

            writer.Write(_transition, (o, w) => o.Serialize(w));
        }

        internal static HistoryStateMetadata<TParent> Deserialize(BinaryReader reader)
        {
            reader.CheckArgNull(nameof(reader));

            var id = reader.ReadNullableString();

            var metadata = new HistoryStateMetadata<TParent>(id);

            metadata.MetadataId = reader.ReadNullableString();

            metadata._isDeep = reader.ReadBoolean();

            metadata._transition = reader.Read(TransitionMetadata<HistoryStateMetadata<TParent>>.Deserialize,
                                                        o => o.Parent = metadata);

            return metadata;
        }

        public override StateType Type => StateType.History;

        protected override IStateMetadata _Parent => this.Parent;

        internal TParent Parent { get; set; }

        protected override IEnumerable<ITransitionMetadata> GetTransitions() => new[] { _transition };

        public TParent _ => this.Parent;

        public TransitionMetadata<HistoryStateMetadata<TParent>> Transition
        {
            get
            {
                _transition = new TransitionMetadata<HistoryStateMetadata<TParent>>();

                _transition.Parent = this;

                _transition.MetadataId = $"{((IModelMetadata)this).MetadataId}.Transition";

                return _transition;
            }
        }

        public HistoryStateMetadata<TParent> IsDeep(bool deep)
        {
            _isDeep = deep;
            return this;
        }

        bool IHistoryStateMetadata.IsDeep => _isDeep;
    }
}
