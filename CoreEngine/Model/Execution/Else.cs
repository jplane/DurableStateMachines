using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx;
using CoreEngine.Abstractions.Model.Execution.Metadata;

namespace CoreEngine.Model.Execution
{
    internal class Else
    {
        private readonly AsyncLazy<ExecutableContent[]> _content;

        public Else(IEnumerable<IExecutableContentMetadata> contentMetadata)
        {
            contentMetadata.CheckArgNull(nameof(contentMetadata));

            _content = new AsyncLazy<ExecutableContent[]>(() =>
            {
                return Task.FromResult(contentMetadata.Select(ExecutableContent.Create).ToArray());
            });
        }

        public async Task Execute(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            context.LogInformation("Start: Else.Execute");

            try
            {
                foreach (var content in await _content)
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
