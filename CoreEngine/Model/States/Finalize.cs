using System;
using SCG=System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Linq;
using CoreEngine.Model.Execution;

namespace CoreEngine.Model.States
{
    internal class Finalize
    {
        private readonly Lazy<SCG.List<ExecutableContent>> _content;

        public Finalize(XElement element)
        {
            _content = new Lazy<SCG.List<ExecutableContent>>(() =>
            {
                var content = new SCG.List<ExecutableContent>();

                foreach (var node in element.Elements())
                {
                    content.Add(ExecutableContent.Create(node));
                }

                return content;
            });
        }
    }
}
