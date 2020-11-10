using CoreEngine.Abstractions.Model.Execution.Metadata;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CoreEngine.Model.Execution
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

            if (metadata is IIfMetadata im)
            {
                content = new If(im);
            }
            else if (metadata is IRaiseMetadata rm)
            {
                content = new Raise(rm);
            }
            else if (metadata is IScriptMetadata sm)
            {
                content = new Script(sm);
            }
            else if (metadata is IForeachMetadata fm)
            {
                content = new Foreach(fm);
            }
            else if (metadata is ILogMetadata lm)
            {
                content = new Log(lm);
            }
            else if (metadata is ISendMetadata smd)
            {
                content = new Send(smd);
            }
            else if (metadata is ICancelMetadata cm)
            {
                content = new Cancel(cm);
            }
            else if (metadata is IAssignMetadata am)
            {
                content = new Assign(am);
            }

            Debug.Assert(content != null);

            return content;
        }

        protected abstract Task _Execute(ExecutionContext context);

        public async Task Execute(ExecutionContext context)
        {
            context.LogInformation($"Start: {this.GetType().Name}.Execute");

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
                context.LogInformation($"End: {this.GetType().Name}.Execute");
            }
        }
    }
}
