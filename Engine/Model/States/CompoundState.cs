using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.States;
using System.Threading.Tasks;

namespace StateChartsDotNet.Model.States
{
    internal abstract class CompoundState : State
    {
        protected Lazy<State[]> _states;

        protected CompoundState(IStateMetadata metadata, State parent)
            : base(metadata, parent)
        {
        }

        public override bool IsSequentialState => true;

        public override IEnumerable<State> GetChildStates()
        {
            Debug.Assert(_states != null);

            return _states.Value.Where(s => s is AtomicState || s is CompoundState || s is ParallelState || s is FinalState);
        }

        public override State GetState(string id)
        {
            var result = base.GetState(id);

            if (result != null)
            {
                return result;
            }

            foreach (var state in GetChildStates())
            {
                result = state.GetState(id);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public override async Task InitDatamodel(ExecutionContextBase context, bool recursive)
        {
            await base.InitDatamodel(context, recursive);

            if (recursive)
            {
                foreach (var child in GetChildStates())
                {
                    await child.InitDatamodel(context, recursive);
                }
            }
        }

        public override void RecordHistory(ExecutionContextBase context)
        {
            context.CheckArgNull(nameof(context));

            foreach (var history in _states.Value.OfType<HistoryState>())
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

                context.StoreHistoryValue(history.Id, predicate);
            }
        }
    }
}
