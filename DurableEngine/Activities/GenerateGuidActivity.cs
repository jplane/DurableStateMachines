using DurableTask.Core;
using System;

namespace StateChartsDotNet.Durable.Activities
{
    internal class GenerateGuidActivity : TaskActivity<string, Guid>
    {
        protected override Guid Execute(TaskContext context, string _)
        {
            return Guid.NewGuid();
        }
    }
}
