using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.Execution;

namespace StateChartsDotNet.Model.Execution
{
    internal class Else<TData>
    {
        private readonly Lazy<ExecutableContent<TData>[]> _content;

        public Else(IElseMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            _content = new Lazy<ExecutableContent<TData>[]>(() =>
            {
                return metadata.GetExecutableContent().Select(ExecutableContent<TData>.Create).ToArray();
            });
        }

        public async Task Execute(ExecutionContextBase<TData> context)
        {
            context.CheckArgNull(nameof(context));

            await context.LogInformationAsync("Start: Else.Execute");

            try
            {
                foreach (var content in _content.Value)
                {
                    await content.ExecuteAsync(context);
                }
            }
            finally
            {
                await context.LogInformationAsync("End: Else.Execute");
            }
        }
    }
}
