using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Linq;
using System.Diagnostics;

namespace CoreEngine.Model.States
{
    internal class ParallelState : CompoundState
    {
        public ParallelState(XElement element, State parent)
            : base(element, parent)
        {
            element.CheckArgNull(nameof(element));

            _states = new Lazy<List<State>>(() =>
            {
                var states = new List<State>();

                bool IsCompoundState(XElement el)
                {
                    return el.ScxmlNameEquals("state") &&
                           el.Elements().Any(ce => ce.ScxmlNameIn("state", "parallel", "final"));
                }

                foreach (var el in element.Elements())
                {
                    if (IsCompoundState(el))
                    {
                        states.Add(new SequentialState(el, this));
                    }
                    else if (el.ScxmlNameEquals("parallel"))
                    {
                        states.Add(new ParallelState(el, this));
                    }
                    else if (el.ScxmlNameEquals("state"))
                    {
                        states.Add(new AtomicState(el, this));
                    }
                    else if (el.ScxmlNameEquals("history"))
                    {
                        states.Add(new HistoryState(el, this));
                    }
                }

                return states;
            });
        }

        public override bool IsParallelState => true;

        public override Transition GetInitialStateTransition()
        {
            throw new NotImplementedException();
        }

        public override bool IsInFinalState(ExecutionContext context, RootState root)
        {
            var childStates = GetChildStates();

            return childStates.All(s => s.IsInFinalState(context, root));
        }
    }
}
