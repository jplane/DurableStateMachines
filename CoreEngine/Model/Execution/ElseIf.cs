using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;
using System;

namespace StateChartsDotNet.CoreEngine.Model.Execution
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

        public async Task<bool> ConditionalExecute(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            context.LogInformation("Start: ElseIf.Execute");

            try
            {
                var result = _cond(context.ScriptData);

                context.LogDebug($"Condition = {result}");

                if (result)
                {
                    foreach (var content in _content.Value)
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
