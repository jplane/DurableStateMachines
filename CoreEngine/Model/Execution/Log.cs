using System.Threading.Tasks;
using CoreEngine.Abstractions.Model.Execution.Metadata;

namespace CoreEngine.Model.Execution
{
    internal class Log : ExecutableContent
    {
        public Log(ILogMetadata metadata)
            : base(metadata)
        {
        }

        protected override async Task _Execute(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            var message = await ((ILogMetadata) _metadata).GetMessage(context.ScriptData);

            context.LogInformation("Log: " + message);
        }
    }
}
