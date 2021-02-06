using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSM.Common;
using DSM.Common.Model.Actions;
using DSM.Engine;

namespace DSM.Engine.Model.Actions
{
    internal class Else
    {
        private readonly Lazy<Action[]> _content;

        public Else(IElseMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            _content = new Lazy<Action[]>(() =>
            {
                return metadata.GetActions().Select(Action.Create).ToArray();
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
