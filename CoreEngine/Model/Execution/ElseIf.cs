using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx;
using CoreEngine.Abstractions.Model.Execution.Metadata;

namespace CoreEngine.Model.Execution
{
    internal class ElseIf
    {
        private readonly AsyncLazy<ExecutableContent[]> _content;
        private readonly string _cond;

        public ElseIf(string condition, Task<IEnumerable<IExecutableContentMetadata>> contentMetadata)
        {
            contentMetadata.CheckArgNull(nameof(contentMetadata));

            _cond = condition;

            _content = new AsyncLazy<ExecutableContent[]>(async () =>
            {
                return (await contentMetadata).Select(ExecutableContent.Create).ToArray();
            });
        }

        public async Task<bool> ConditionalExecute(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            context.LogInformation("Start: ElseIf.Execute");

            try
            {
                var result = await context.Eval<bool>(_cond);

                context.LogDebug($"Condition = {result}");

                if (result)
                {
                    foreach (var content in await _content)
                    {
                        await content.Execute(context);
                    }
                }

                return result;
            }
            finally
            {
                context.LogInformation("End: ElseIf.Execute");
            }
        }
    }
}
