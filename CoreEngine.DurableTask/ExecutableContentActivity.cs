using DurableTask.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.DurableTask
{
    internal class ExecutableContentActivity : AsyncTaskActivity<string, bool>
    {
        private readonly Func<ExecutionContext, Task> _func;
        private readonly ExecutionContext _executionContext;

        public ExecutableContentActivity(Func<ExecutionContext, Task> func, ExecutionContext context)
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
