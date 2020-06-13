using Confluent.Kafka;
using Core.Domain;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Threading.Tasks;

namespace Product.Service.Events
{
    public interface IProducer
    {
        Task Publish(string topic, BaseEvent content);
    }

    public class Producer : IProducer
    {
        private readonly ProducerConfig _config;

        public Producer(IOptions<Config.Settings> settings)
        {
            _config = new ProducerConfig
            {
                BootstrapServers = settings.Value.ConnectionStrings.BrokerConnectionString,
                ClientId = Dns.GetHostName(),
                EnableIdempotence = true,
                EnableDeliveryReports = true
            };
        }

        public async Task Publish(string topic, BaseEvent content)
        {
            using var producer = new ProducerBuilder<string, string>(_config)
            .Build();

            var messageValue = JsonConvert.SerializeObject(content);

            var message = new Message<string, string> { Key = content.EventId.ToString(), Value = messageValue };

            await producer.ProduceAsync(topic, message);
        }
    }
}
