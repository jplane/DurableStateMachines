using StateChartsDotNet.CoreEngine.Abstractions;
using StateChartsDotNet.CoreEngine.Abstractions.MessageTransports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.MessageTransports
{
    internal class InMemoryQueueTransport : IMessageTransport
    {
        private readonly Queue<Message> _queue;

        public InMemoryQueueTransport()
        {
            _queue = new Queue<Message>();
        }

        public Task<bool> HasMessageAsync()
        {
            return Task.FromResult(_queue.Count > 0);
        }

        public Task SendAsync(Message message)
        {
            message.CheckArgNull(nameof(message));

            _queue.Enqueue(message);

            return Task.CompletedTask;
        }

        public Task<bool> TryReceiveAsync(out Message message)
        {
            var result = false;

            lock(_queue)
            {
                result = _queue.TryDequeue(out message);
            }

            return Task.FromResult(result);
        }
    }
}
