using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CoreEngine.Model.Execution;

namespace CoreEngine.Model.States
{
    internal class OnEntryExit
    {
        private readonly Lazy<List<ExecutableContent>> _content;
        private readonly bool _isEntry;

        public OnEntryExit(XElement element)
        {
            element.CheckArgNull(nameof(element));

            _isEntry = element.Name.LocalName.ToLowerInvariant() == "onentry";

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

        public async Task Execute(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            var name = _isEntry ? "OnEntry" : "OnExit";

            context.LogInformation($"Start: {name}");

            try
            {
                foreach (var content in _content.Value)
                {
                    await content.Execute(context);
                }
            }
            finally
            {
                context.LogInformation($"End: {name}");
            }
        }
    }
}
