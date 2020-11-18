using DurableTask.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace StateChartsDotNet.CoreEngine.DurableTask
{
    internal class GenerateGuidActivity : TaskActivity<string, Guid>
    {
        protected override Guid Execute(TaskContext context, string _)
        {
            return Guid.NewGuid();
        }
    }
}
