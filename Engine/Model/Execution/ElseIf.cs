using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StateChartsDotNet.Common.Model.Execution;
using System;

namespace StateChartsDotNet.Model.Execution
{
    internal class ElseIf
    {
        private readonly Lazy<ExecutableContent[]> _content;
        private readonly Func<dynamic, bool> _cond;

        public ElseIf(Func<dynamic, bool> condition, IEnumerable<IExecutableContentMetadata> contentMetadata)
        {
            contentMetadata.CheckArgNull(nameof(contentMetadata));

            _cond = condition;

            _content = new Lazy<ExecutableContent[]>(() =>
            {
                return contentMetadata.Select(ExecutableContent.Create).ToArray();
            });
        }

        public async Task<bool> ConditionalExecuteAsync(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            await context.LogInformationAsync("Start: ElseIf.Execute");

            try
            {
                var result = _cond(context.ScriptData);

                await context.LogDebugAsync($"Condition = {result}");

                if (result)
                {
                    foreach (var content in _content.Value)
                    {
                        await content.ExecuteAsync(context);
                    }
                }

                return result;
            }
            finally
            {
                await context.LogInformationAsync("End: ElseIf.Execute");
            }
        }
    }
}
