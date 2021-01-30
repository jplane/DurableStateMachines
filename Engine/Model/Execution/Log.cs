using System.Diagnostics;
using System.Threading.Tasks;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.Execution;

namespace StateChartsDotNet.Model.Execution
{
    internal class Log : ExecutableContent
    {
        public Log(ILogMetadata metadata)
            : base(metadata)
        {
        }

        protected override async Task _ExecuteAsync(ExecutionContextBase context)
        {
            context.CheckArgNull(nameof(context));

            var metadata = (ILogMetadata) _metadata;

            var message = metadata.GetMessage(context.ExecutionData);

            await context.LogInformationAsync("Log: " + message);
        }
    }
}
