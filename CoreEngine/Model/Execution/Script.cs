using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.Model.Execution
{
    internal class Script : ExecutableContent
    {
        public Script(IScriptMetadata metadata)
            : base(metadata)
        {
        }

        protected override Task _Execute(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            var metadata = (IScriptMetadata) _metadata;

            return context.ExecuteContent(metadata.UniqueId, ec =>
            {
                Debug.Assert(ec != null);

                metadata.Execute(ec.ScriptData);

                return Task.CompletedTask;
            });
        }
    }
}
