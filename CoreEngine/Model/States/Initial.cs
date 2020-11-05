using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace CoreEngine.Model.States
{
    internal class Initial
    {
        private readonly Lazy<Transition> _transition;

        public Initial(XElement element, State parent)
        {
            _transition = new Lazy<Transition>(() =>
            {
                var node = element.ScxmlElement("transition");

                return new Transition(node, parent);
            });
        }

        public Transition Transition => _transition.Value;
    }
}
