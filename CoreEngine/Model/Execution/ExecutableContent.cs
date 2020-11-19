using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.Model.Execution
{
    internal abstract class ExecutableContent
    {
        protected readonly IExecutableContentMetadata _metadata;

        protected ExecutableContent(IExecutableContentMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            _metadata = metadata;
        }

        public static ExecutableContent Create(IExecutableContentMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            ExecutableContent content = null;

            switch (metadata)
            {
                case IIfMetadata @if:
                    content = new If(@if);
                    break;
                case IRaiseMetadata raise:
                    content = new Raise(raise);
                    break;
                case IScriptMetadata script:
                    content = new Script(script);
                    break;
                case IForeachMetadata @foreach:
                    content = new Foreach(@foreach);
                    break;
                case ILogMetadata log:
                    content = new Log(log);
                    break;
                case ISendMessageMetadata send:
                    content = new SendMessage(send);
                    break;
                case ICancelMetadata cancel:
                    content = new Cancel(cancel);
                    break;
                case IAssignMetadata assign:
                    content = new Assign(assign);
                    break;
                default:
                    throw new ArgumentException(message: "Executable Metadata is not a recognized type", paramName: nameof(metadata));
            }

            Debug.Assert(content != null);

            return content;
        }

        protected abstract Task _Execute(ExecutionContext context);

        public async Task Execute(ExecutionContext context)
        {
            await context.LogInformation($"Start: {this.GetType().Name}.Execute");

            try
            {
                await _Execute(context);
            }
            catch (Exception ex)
            {
                context.EnqueueExecutionError(ex);
            }
            finally
            {
                await context.LogInformation($"End: {this.GetType().Name}.Execute");
            }
        }
    }
}
