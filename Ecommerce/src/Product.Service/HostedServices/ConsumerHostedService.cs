using Microsoft.Extensions.Hosting;
using Product.Service.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Product.Service.HostedServices
{
    public class ConsumerHostedService : IHostedService
    {
        private readonly Consumer _consumer;
        public ConsumerHostedService(Consumer consumer)
        {
            _consumer = consumer;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _consumer.Consume();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _consumer.EndConsumer();

            return Task.CompletedTask;
        }
    }
}
