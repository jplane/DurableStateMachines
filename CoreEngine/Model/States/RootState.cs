using System;
using SCG=System.Collections.Generic;
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

        private readonly string _initial;
        private readonly string _name;
        private readonly string _xmlns;
        private readonly string _version;
        private readonly string _datamodelType;
        private readonly Databinding _binding;

        public RootState(XElement element)
            : base(element, null)
        {
            _initial = element.Attribute("initial")?.Value ?? string.Empty;

            _name = element.Attribute("name")?.Value ?? string.Empty;

            _xmlns = element.Attribute("xmlns").Value;

            _version = element.Attribute("version").Value;

            _datamodelType = element.Attribute("datamodel")?.Value ?? "csharp";

            _binding = (Databinding) Enum.Parse(typeof(Databinding),
                                                element.Attribute("binding")?.Value ?? "early",
                                                true);

            _states = new Lazy<SCG.List<State>>(() =>
            {
                var states = new SCG.List<State>();

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
                }

                return states;
            });

            _script = new Lazy<Script>(() =>
            {
                var node = element.Element("script");

                return node == null ? null : new Script(node);
            });
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
            if (!string.IsNullOrWhiteSpace(_initial))
            {
                return new Transition(_initial, this);
            }
            else
            {
                return base.GetInitialStateTransition();
            }
        }

        public override List<State> GetChildStates()
        {
            return new List<State>(_states.Value.Where(s => s is AtomicState ||
                                                            s is CompoundState ||
                                                            s is ParallelState ||
                                                            s is FinalState));
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
    }

    internal enum Databinding
    {
        Early,
        Late
    }
}
