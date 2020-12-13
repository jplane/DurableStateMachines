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
    internal class SendMessageActivity : AsyncTaskActivity<(string,
                                                            string,
                                                            string,
                                                            object,
                                                            string,
                                                            IReadOnlyDictionary<string, object>), object>
    {
        private readonly Func<string, ExternalServiceDelegate> _getDelegate;
        private readonly CancellationToken _token;

        public SendMessageActivity(Func<string, ExternalServiceDelegate> getDelegate, CancellationToken token)
        {
            getDelegate.CheckArgNull(nameof(getDelegate));

            _getDelegate = getDelegate;
            _token = token;
        }

        protected override async Task<object> ExecuteAsync(TaskContext context,
                                                           (string,
                                                            string,
                                                            string,
                                                            object,
                                                            string,
                                                            IReadOnlyDictionary<string, object>) input)
        {
            var serviceType = input.Item1;
            var target = input.Item2;
            var messageName = input.Item3;
            var content = input.Item4;
            var correlationId = input.Item5;
            var parameters = input.Item6;

            Debug.Assert(!string.IsNullOrWhiteSpace(serviceType));
            Debug.Assert(!string.IsNullOrWhiteSpace(target));
            Debug.Assert(parameters != null);

            var service = _getDelegate(serviceType);

            if (service == null)
            {
                throw new InvalidOperationException("Unable to resolve external service type: " + serviceType);
            }

            await service(target, messageName, content, correlationId, parameters, _token).ConfigureAwait(false);

            return null;
        }
    }
}
