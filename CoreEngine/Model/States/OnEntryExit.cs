using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using CoreEngine.Model.Execution;

namespace CoreEngine.Model.States
{
    internal class OnEntryExit
    {
        private readonly Lazy<List<ExecutableContent>> _content;

        public OnEntryExit(XElement element)
        {
            _content = new Lazy<List<ExecutableContent>>(() =>
            {
                var content = new List<ExecutableContent>();

                foreach (var node in element.Elements())
                {
                    content.Add(ExecutableContent.Create(node));
                }

                return content;
            });
        }

        public void Execute(ExecutionContext context)
        {
            foreach (var content in _content.Value)
            {
                content.Execute(context);
            }
        }
    }
}
