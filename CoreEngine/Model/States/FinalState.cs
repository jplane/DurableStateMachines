using StateChartsDotNet.CoreEngine.Abstractions.Model.States;
using StateChartsDotNet.CoreEngine.Model.DataManipulation;
using Nito.AsyncEx;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace StateChartsDotNet.CoreEngine.Model.States
{
    internal class FinalState : State
    {
        private readonly AsyncLazy<Content> _content;
        private readonly AsyncLazy<Param[]> _params;

        public FinalState(IFinalStateMetadata metadata, State parent)
            : base(metadata, parent)
        {
            metadata.CheckArgNull(nameof(metadata));

            _content = new AsyncLazy<Content>(async () =>
            {
                var meta = await metadata.GetContent();

                if (meta != null)
                    return new Content(meta);
                else
                    return null;
            });

            _params = new AsyncLazy<Param[]>(async () =>
            {
                return (await metadata.GetParams()).Select(pm => new Param(pm)).ToArray();
            });
        }

        public override bool IsFinalState => true;

        public override Task Invoke(ExecutionContext context, RootState root)
        {
            throw new NotImplementedException();
        }

        public override Task InitDatamodel(ExecutionContext context, bool recursive)
        {
            return Task.CompletedTask;
        }
    }
}
