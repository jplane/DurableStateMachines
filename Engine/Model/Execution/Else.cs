using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSM.Common;
using DSM.Common.Model.Execution;
using DSM.Engine;

namespace DSM.Engine.Model.Execution
{
    internal class Else
    {
        private readonly Lazy<ExecutableContent[]> _content;

        public Else(IElseMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            _content = new Lazy<ExecutableContent[]>(() =>
            {
                return metadata.GetExecutableContent().Select(ExecutableContent.Create).ToArray();
            });
        }

        public async Task Execute(ExecutionContextBase context)
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
