using DurableTask.Core;
using System;
using System.Threading.Tasks;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.Execution;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;

namespace StateChartsDotNet.Durable.Activities
{
    internal class QueryActivity : AsyncTaskActivity<(string,
                                                      string,
                                                      IReadOnlyDictionary<string, object>), object>
    {
        private readonly Func<string, ExternalQueryDelegate> _getDelegate;
        private readonly CancellationToken _token;

        public QueryActivity(Func<string, ExternalQueryDelegate> getDelegate, CancellationToken token)
        {
            getDelegate.CheckArgNull(nameof(getDelegate));

            _getDelegate = getDelegate;
            _token = token;
        }

        protected override async Task<object> ExecuteAsync(TaskContext context,
                                                           (string,
                                                            string,
                                                            IReadOnlyDictionary<string, object>) input)
        {
            var queryType = input.Item1;
            var target = input.Item2;
            var parameters = input.Item3;

            Debug.Assert(!string.IsNullOrWhiteSpace(queryType));
            Debug.Assert(!string.IsNullOrWhiteSpace(target));
            Debug.Assert(parameters != null);

            var query = _getDelegate(queryType);

            if (query == null)
            {
                throw new InvalidOperationException("Unable to resolve external query type: " + queryType);
            }

            return await query(target, parameters, _token);
        }
    }
}
