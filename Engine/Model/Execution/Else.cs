using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StateChartsDotNet.Common.Model.Execution;

namespace StateChartsDotNet.Model.Execution
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

            await context.LogInformation("Start: Else.Execute");

            try
            {
                foreach (var content in _content.Value)
                {
                    await content.Execute(context);
                }
            }
            finally
            {
                await context.LogInformation("End: Else.Execute");
            }
        }
    }
}
