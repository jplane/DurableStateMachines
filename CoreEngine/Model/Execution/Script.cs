using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;
using System;
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

            ((IScriptMetadata) _metadata).Execute(context.ScriptData);

            return Task.CompletedTask;
        }
    }
}
