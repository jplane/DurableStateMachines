using DurableTask.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace StateChartsDotNet.CoreEngine.DurableTask
{
    internal class LoggerActivity : TaskActivity<(string, string), bool>
    {
        private readonly ILogger _logger;

        public LoggerActivity(ILogger logger)
        {
            _logger = logger;
        }

        protected override bool Execute(TaskContext context, (string, string) tuple)
        {
            switch(tuple.Item1.ToLowerInvariant())
            {
                case "information":
                    _logger?.LogInformation(tuple.Item2);
                    break;

                case "debug":
                    _logger?.LogDebug(tuple.Item2);
                    break;
            }

            return true;
        }
    }
}
