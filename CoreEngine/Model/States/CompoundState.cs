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

        public override void InitDatamodel(ExecutionContext context, bool recursive)
        {
            base.InitDatamodel(context, recursive);

            if (recursive)
            {
                foreach (var child in GetChildStates())
                {
                    child.InitDatamodel(context, recursive);
                }
            }
        }

        public override void RecordHistory(ExecutionContext context)
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

                context.StoreHistoryValue(history.Id, context.Configuration.Where(predicate));
            }
        }
    }
}
