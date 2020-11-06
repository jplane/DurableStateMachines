using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Linq;
using System.Data;
using CoreEngine.Model.Execution;

namespace CoreEngine.Model.States
{
    internal class RootState : CompoundState
    {
        private readonly Lazy<Script> _script;
        private readonly Lazy<Transition> _initialTransition;
        
        private readonly string _name;
        private readonly string _xmlns;
        private readonly string _version;
        private readonly string _datamodelType;
        private readonly Databinding _binding;

        public RootState(XElement element)
            : base(element, null)
        {
            element.CheckArgNull(nameof(element));

            _name = element.Attribute("name")?.Value ?? string.Empty;

            _xmlns = element.Attribute("xmlns").Value;

            _version = element.Attribute("version").Value;

            _datamodelType = element.Attribute("datamodel")?.Value ?? "csharp";

            _binding = (Databinding) Enum.Parse(typeof(Databinding),
                                                element.Attribute("binding")?.Value ?? "early",
                                                true);

            _initialTransition = new Lazy<Transition>(() =>
            {
                var attr = element.Attribute("initial");

                return attr == null ? null : new Transition(attr, this);
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
                }

                return states;
            });

            _script = new Lazy<Script>(() =>
            {
                var node = element.ScxmlElement("script");

                return node == null ? null : new Script(node);
            });
        }

        public void ExecuteScript(ExecutionContext context)
        {
            _script.Value?.Execute(context);
        }

        public Databinding Binding => _binding;

        public override string Id => "scxml_root";

        public override bool IsScxmlRoot => true;

        public override void Invoke(ExecutionContext context, RootState root)
        {
            throw new InvalidOperationException("Unexpected invocation.");
        }

        public override Transition GetInitialStateTransition()
        {
            return _initialTransition.Value ?? base.GetInitialStateTransition();
        }

        public override void RecordHistory(ExecutionContext context)
        {
            throw new NotImplementedException();
        }
    }

    internal enum Databinding
    {
        Early,
        Late
    }
}
