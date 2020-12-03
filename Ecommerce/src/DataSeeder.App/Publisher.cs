using Confluent.Kafka;
using Core.Domain;
using Newtonsoft.Json;
using System.Net;
using System.Threading.Tasks;

namespace DataSeeder.App
{
    public interface IPublisher
    {
        Task Publish(string topic, BaseEvent content);
    }

    public class Publisher : IPublisher
    {
        private readonly ProducerConfig _config;

        public Publisher(string connectionString)
        {
            _config = new ProducerConfig
            {
                BootstrapServers = "localhost:9092", // connectionString
                ClientId = Dns.GetHostName(),
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
