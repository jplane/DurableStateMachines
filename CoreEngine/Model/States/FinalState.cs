using StateChartsDotNet.CoreEngine.Abstractions.Model.States;
using StateChartsDotNet.CoreEngine.Model.DataManipulation;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace StateChartsDotNet.CoreEngine.Model.States
{
    internal class FinalState : State
    {
        private readonly Lazy<Content> _content;
        private readonly Lazy<Param[]> _params;

        public FinalState(IFinalStateMetadata metadata, State parent)
            : base(metadata, parent)
        {
            metadata.CheckArgNull(nameof(metadata));

            _content = new Lazy<Content>(() =>
            {
                var meta = metadata.GetContent();

                if (meta != null)
                    return new Content(meta);
                else
                    return null;
            });

            _params = new Lazy<Param[]>(() =>
            {
                return metadata.GetParams().Select(pm => new Param(pm)).ToArray();
            });
        }

        public override bool IsFinalState => true;

        public override Task Invoke(ExecutionContext context, RootState root)
        {
            throw new NotImplementedException();
        }

        public override void InitDatamodel(ExecutionContext context, bool recursive)
        {
        }
    }
}
