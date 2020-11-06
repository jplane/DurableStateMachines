using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Linq;

namespace CoreEngine.Model.States
{
    class SequentialState : CompoundState
    {
        private readonly Lazy<Transition> _initialTransition;
        private readonly Lazy<Initial> _initialElement;

        public SequentialState(XElement element, State parent)
            : base(element, parent)
        {
            element.CheckArgNull(nameof(element));

            _initialTransition = new Lazy<Transition>(() =>
            {
                var attr = element.Attribute("initial");

                return attr == null ? null : new Transition(attr, this);
            });

            _initialElement = new Lazy<Initial>(() =>
            {
                var node = element.ScxmlElement("initial");

                return node == null ? null : new Initial(node, this);
            });

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
                    else if (el.ScxmlNameEquals("final"))
                    {
                        states.Add(new FinalState(el, this));
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

        public override bool IsSequentialState => true;

        public override Transition GetInitialStateTransition()
        {
            if (_initialTransition.Value != null)
            {
                return _initialTransition.Value;
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

        public override bool IsInFinalState(ExecutionContext context, RootState root)
        {
            var childStates = GetChildStates();

            return childStates.Any(s => s.IsFinalState && context.Configuration.Contains(s));
        }
    }
}
