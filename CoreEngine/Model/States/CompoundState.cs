using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using StateChartsDotNet.CoreEngine.Abstractions.Model.States;
using Nito.AsyncEx;

namespace StateChartsDotNet.CoreEngine.Model.States
{
    internal abstract class CompoundState : State
    {
        protected AsyncLazy<State[]> _states;

        protected CompoundState(IStateMetadata metadata, State parent)
            : base(metadata, parent)
        {
        }

        public override bool IsSequentialState => true;

        public override async Task<IEnumerable<State>> GetChildStates()
        {
            Debug.Assert(_states != null);

            return (await _states).Where(s => s is AtomicState ||
                                              s is CompoundState ||
                                              s is ParallelState ||
                                              s is FinalState);
        }

        public override async Task<State> GetState(string id)
        {
            var result = await base.GetState(id);

            if (result != null)
            {
                return result;
            }

            foreach (var state in await GetChildStates())
            {
                result = await state.GetState(id);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public override async Task InitDatamodel(ExecutionContext context, bool recursive)
        {
            await base.InitDatamodel(context, recursive);

            if (recursive)
            {
                foreach (var child in await GetChildStates())
                {
                    await child.InitDatamodel(context, recursive);
                }
            }
        }

        public override async Task RecordHistory(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            foreach (var history in (await _states).OfType<HistoryState>())
            {
                Func<State, bool> predicate;

                if (history.IsDeepHistoryState)
                {
                    predicate = s => s.IsAtomic && s.IsDescendent(this);
                }
                else
                {
                    predicate = s => string.Compare(_parent.Id, this.Id, StringComparison.InvariantCultureIgnoreCase) == 0;
                }

                context.StoreHistoryValue(history.Id, context.Configuration.Where(predicate));
            }
        }
    }
}
