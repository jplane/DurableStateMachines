using CoreEngine.Abstractions.Model.Execution.Metadata;
using System;
using System.Threading.Tasks;

namespace CoreEngine.Model.Execution
{
    internal class Script : ExecutableContent
    {
        public Script(IScriptMetadata metadata)
            : base(metadata)
        {
        }

        protected override async Task _Execute(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            await ((IScriptMetadata) _metadata).Execute(context.ScriptData);
        }
    }
}
