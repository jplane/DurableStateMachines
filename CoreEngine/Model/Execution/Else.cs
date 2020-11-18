using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;

namespace StateChartsDotNet.CoreEngine.Model.Execution
{
    internal class Else
    {
        private readonly Lazy<ExecutableContent[]> _content;

        public Else(IEnumerable<IExecutableContentMetadata> contentMetadata)
        {
            contentMetadata.CheckArgNull(nameof(contentMetadata));

            _content = new Lazy<ExecutableContent[]>(() =>
            {
                return contentMetadata.Select(ExecutableContent.Create).ToArray();
            });
        }

        public async Task Execute(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            context.LogInformation("Start: Else.Execute");

            try
            {
                foreach (var content in _content.Value)
                {
                    await content.Execute(context);
                }
            }
            finally
            {
                context.LogInformation("End: Else.Execute");
            }
        }
    }
}
