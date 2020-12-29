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
        private HistoryType _type;

        internal HistoryStateMetadata(string id)
            : base(id)
        {
            _type = HistoryType.Deep;
        }

        internal override void Serialize(BinaryWriter writer)
        {
            writer.CheckArgNull(nameof(writer));

            base.Serialize(writer);

            writer.Write((int) _type);

            writer.Write(_transition, (o, w) => o.Serialize(w));
        }

        internal static HistoryStateMetadata<TParent> Deserialize(BinaryReader reader)
        {
            reader.CheckArgNull(nameof(reader));

            var id = reader.ReadString();

            var metadata = new HistoryStateMetadata<TParent>(id);

            metadata.MetadataId = reader.ReadString();

            metadata._transition = reader.Read(TransitionMetadata<HistoryStateMetadata<TParent>>.Deserialize,
                                                        o => o.Parent = metadata);

            return metadata;
        }

        protected override IStateMetadata _Parent => this.Parent;

        internal TParent Parent { get; set; }

        protected override IEnumerable<ITransitionMetadata> GetTransitions() => new[] { _transition };

        public TParent Attach()
        {
            return this.Parent;
        }

        public TransitionMetadata<HistoryStateMetadata<TParent>> Transition()
        {
            _transition = new TransitionMetadata<HistoryStateMetadata<TParent>>();

            _transition.Parent = this;

            _transition.MetadataId = $"{((IModelMetadata)this).MetadataId}.Transition";

            return _transition;
        }

        public HistoryStateMetadata<TParent> Type(HistoryType type)
        {
            _type = type;

            return this;
        }

        HistoryType IHistoryStateMetadata.Type => _type;
    }
}
