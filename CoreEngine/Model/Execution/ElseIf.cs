using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx;
using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;
using System;

namespace StateChartsDotNet.CoreEngine.Model.Execution
{
    internal class ElseIf
    {
        private readonly AsyncLazy<ExecutableContent[]> _content;
        private readonly Func<dynamic, Task<bool>> _cond;

        public ElseIf(Func<dynamic, Task<bool>> condition, Task<IEnumerable<IExecutableContentMetadata>> contentMetadata)
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
                var result = await _cond(context.ScriptData);

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
