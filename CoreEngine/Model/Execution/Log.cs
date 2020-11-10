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

            if (!string.IsNullOrWhiteSpace(((ILogMetadata) _metadata).Message))
            {
                var message = await context.Eval<string>(((ILogMetadata) _metadata).Message);

                context.LogInformation("Log: " + message);
            }
        }
    }
}
