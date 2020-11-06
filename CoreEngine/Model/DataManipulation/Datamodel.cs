using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Linq;
using CoreEngine.Model.States;

namespace CoreEngine.Model.DataManipulation
{
    internal class Datamodel
    {
        protected readonly Lazy<List<Data>> _data;

        public Datamodel(XElement element)
        {
            element.CheckArgNull(nameof(element));

            _data = new Lazy<List<Data>>(() =>
            {
                var nodes = element.ScxmlElements("data");

                return new List<Data>(nodes.Select(n => new Data(n)));
            });
        }

        public void Init(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            foreach (var data in _data.Value)
            {
                data.Init(context);
            }
        }
    }
}
