using CoreEngine.Model.DataManipulation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CoreEngine.Model.States
{
    internal class FinalState : State
    {
        private readonly Lazy<Donedata> _donedata;

        public FinalState(XElement element, State parent)
            : base(element, parent)
        {
            element.CheckArgNull(nameof(element));

            _donedata = new Lazy<Donedata>(() =>
            {
                var node = element.ScxmlElement("donedata");

                return node == null ? null : new Donedata(node);
            });
        }

        public override bool IsFinalState => true;

        public override void Invoke(ExecutionContext context, RootState root)
        {
            throw new NotImplementedException();
        }

        public override Task InitDatamodel(ExecutionContext context, bool recursive)
        {
            return Task.CompletedTask;
        }
    }
}
