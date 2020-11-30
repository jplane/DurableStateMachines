using DurableTask.Core;
using System;
using System.Threading.Tasks;
using StateChartsDotNet.Common;

namespace StateChartsDotNet.Durable
{
    internal class ExecutableContentActivity : AsyncTaskActivity<string, bool>
    {
        private readonly Func<StateChartsDotNet.ExecutionContext, Task> _func;
        private readonly StateChartsDotNet.ExecutionContext _executionContext;

        public ExecutableContentActivity(Func<StateChartsDotNet.ExecutionContext, Task> func, StateChartsDotNet.ExecutionContext context)
        {
            func.CheckArgNull(nameof(func));
            context.CheckArgNull(nameof(context));

            _func = func;
            _executionContext = context;
        }

        protected override Task<bool> ExecuteAsync(TaskContext context, string _)
        {
            return _func(_executionContext).ContinueWith(_ => true);
        }
    }
}
