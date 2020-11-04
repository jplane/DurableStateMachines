using System;
using SCG=System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Linq;
using CoreEngine.Model.States;

namespace CoreEngine.Model.DataManipulation
{
    internal class Datamodel
    {
        protected readonly Lazy<SCG.List<Data>> _data;

        public Datamodel(XElement element)
        {
            _data = new Lazy<SCG.List<Data>>(() =>
            {
                var nodes = element.Elements("data");

                return new SCG.List<Data>(nodes.Select(n => new Data(n)));
            });
        }

        public void Init(ExecutionContext context)
        {
            foreach (var data in _data.Value)
            {
                data.Init(context);
            }
        }
    }
}
