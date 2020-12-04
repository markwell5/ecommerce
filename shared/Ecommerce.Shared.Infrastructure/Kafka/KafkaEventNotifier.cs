using Confluent.Kafka;
using Ecommerce.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ecommerce.Shared.Infrastructure.Kafka
{
    public class KafkaEventNotifier : IEventNotifier
    {
        private readonly ProducerConfig _producerConfig;
        private readonly ILogger _logger;

        public KafkaEventNotifier(IOptions<KafkaSettings> settings, ILogger<KafkaEventNotifier> logger)
        {
            _producerConfig = new ProducerConfig
            {
                BootstrapServers = settings.Value.BootstrapServers,
                ClientId = Dns.GetHostName(),
                EnableIdempotence = true,
                EnableDeliveryReports = true,
            };

            _logger = logger;
        }

        public async Task Notify(IEvent content)
        {
            using var producer = new ProducerBuilder<Null, string>(_producerConfig)
                .SetErrorHandler((p, err) =>
                {
                    _logger.LogError(err.Reason);
                })
                 .Build();

            var message = new Message<Null, string> { Value = JsonSerializer.Serialize(content) };
            await producer.ProduceAsync(content.EventName.ToLower(), message);
        }
    }
}
