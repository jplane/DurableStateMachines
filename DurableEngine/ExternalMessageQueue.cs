using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StateChartsDotNet.Durable
{
    internal class ExternalMessageQueue
    {
        private readonly Queue<ExternalMessage> _queue;

        private TaskCompletionSource<bool> _messageAvailable;

        public ExternalMessageQueue()
        {
            _queue = new Queue<ExternalMessage>();
            _messageAvailable = new TaskCompletionSource<bool>();
        }

        public void Enqueue(ExternalMessage message)
        {
            message.CheckArgNull(nameof(message));

            _queue.Enqueue(message);

            _messageAvailable.TrySetResult(true);
        }

        public async Task<ExternalMessage> DequeueAsync(CancellationToken token = default)
        {
            using (token.Register(() => _messageAvailable.SetCanceled()))
            {
                await _messageAvailable.Task;
            }

            Debug.Assert(_queue.Count > 0);

            var msg = _queue.Dequeue();

            if (_queue.Count == 0)
            {
                _messageAvailable = new TaskCompletionSource<bool>();
            }

            return msg;
        }
    }
}
