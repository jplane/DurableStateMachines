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

        protected override Task _ExecuteAsync(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            var metadata = (ILogMetadata) _metadata;

            return context.ExecuteContentAsync(metadata.UniqueId, async ec =>
            {
                Debug.Assert(ec != null);

                var message = metadata.GetMessage(ec.ScriptData);

                await ec.LogInformationAsync("Log: " + message);
            });
        }
    }
}
