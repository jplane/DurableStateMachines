using System.Threading.Tasks;
using DSM.Common;
using DSM.Common.Model.Execution;
using DSM.Engine;

namespace DSM.Engine.Model.Execution
{
    internal class Log : Action
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
