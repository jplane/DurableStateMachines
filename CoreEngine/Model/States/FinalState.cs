using CoreEngine.Abstractions.Model.States.Metadata;
using CoreEngine.Model.DataManipulation;
using Nito.AsyncEx;
using System;
using System.Threading.Tasks;

namespace CoreEngine.Model.States
{
    internal class FinalState : State
    {
        private readonly AsyncLazy<Donedata> _donedata;

        public FinalState(IFinalStateMetadata metadata, State parent)
            : base(metadata, parent)
        {
            metadata.CheckArgNull(nameof(metadata));

            _donedata = new AsyncLazy<Donedata>(async () =>
            {
                var meta = await metadata.GetDonedata();

                if (meta != null)
                    return new Donedata(meta);
                else
                    return null;
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
