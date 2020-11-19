using System.Diagnostics;
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

            var metadata = (ILogMetadata) _metadata;

            return context.ExecuteContent(metadata.UniqueId, ec =>
            {
                Debug.Assert(ec != null);

                var message = metadata.GetMessage(ec.ScriptData);

                ec.LogInformation("Log: " + message);

                return Task.CompletedTask;
            });
        }
    }
}
