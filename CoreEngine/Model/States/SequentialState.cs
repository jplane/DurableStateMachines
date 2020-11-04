using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Linq;

namespace CoreEngine.Model.States
{
    class SequentialState : CompoundState
    {
        private readonly string _initialAttribute;
        private readonly Lazy<Initial> _initialElement;

        public SequentialState(XElement element, State parent)
            : base(element, parent)
        {
            _initialAttribute = element.Attribute("initial")?.Value ?? string.Empty;

            _initialElement = new Lazy<Initial>(() =>
            {
                var node = element.Element("initial");

                return node == null ? null : new Initial(node, this);
            });

            _states = new Lazy<List<State>>(() =>
            {
                var states = new List<State>();

                bool IsCompoundState(XElement el)
                {
                    return el.Name == "state" &&
                           el.Elements().Any(ce => ce.Name == "state" ||
                                                   ce.Name == "parallel" ||
                                                   ce.Name == "final");
                }

                foreach (var el in element.Elements())
                {
                    if (IsCompoundState(el))
                    {
                        states.Add(new SequentialState(el, this));
                    }
                    else if (el.Name == "parallel")
                    {
                        states.Add(new ParallelState(el, this));
                    }
                    else if (el.Name == "final")
                    {
                        states.Add(new FinalState(el, this));
                    }
                    else if (el.Name == "state")
                    {
                        states.Add(new AtomicState(el, this));
                    }
                    else if (el.Name == "history")
                    {
                        states.Add(new HistoryState(el, this));
                    }
                }

                return states;
            });
        }

        public override bool IsSequentialState => true;

        public override Transition GetInitialStateTransition()
        {
            if (!string.IsNullOrWhiteSpace(_initialAttribute))
            {
                return new Transition(_initialAttribute, this);
            }
            else if (_initialElement.Value != null)
            {
                return _initialElement.Value.Transition;
            }
            else
            {
                return base.GetInitialStateTransition();
            }
        }

        public override void RecordHistory(ExecutionContext context, RootState root)
        {
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

        public override bool IsInFinalState(ExecutionContext context, RootState root)
        {
            var childStates = GetChildStates();

            return childStates.Any(s => s.IsFinalState && context.Configuration.Contains(s));
        }
    }
}
