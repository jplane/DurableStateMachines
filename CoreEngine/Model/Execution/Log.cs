using System.Threading.Tasks;
using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;

namespace StateChartsDotNet.CoreEngine.Model.Execution
{
    internal class Log : ExecutableContent
    {
        public Log(ILogMetadata metadata)
            : base(metadata)
        {
        }

        protected override Task _Execute(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            var message = ((ILogMetadata) _metadata).GetMessage(context.ScriptData);

            context.LogInformation("Log: " + message);

            return Task.CompletedTask;
        }
    }
}
