using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Linq;
using CoreEngine.Model.Execution;

namespace CoreEngine.Model.States
{
    internal class Finalize
    {
        private readonly Lazy<List<ExecutableContent>> _content;

        public Finalize(XElement element)
        {
            element.CheckArgNull(nameof(element));

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
    }
}
