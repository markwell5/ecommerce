using Confluent.Kafka;
using Core.Domain;
using Core.Domain.Commands;
using MediatR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Product.Service.Commands;
using System.Threading.Tasks;

namespace Product.Service.Events
{
    public class Consumer
    {
        private readonly IMediator _mediator;
        private readonly ConsumerConfig _config;
        private bool run = true;

        public Consumer(IMediator mediator, IOptions<Config.Settings> settings)
        {
            _mediator = mediator;

            _config = new ConsumerConfig
            {
                BootstrapServers = settings.Value.ConnectionStrings.BrokerConnectionString,
                GroupId = "product-service",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
        }

        public async Task Consume()
        {
            using var consumer = new ConsumerBuilder<Ignore, string>(_config).Build();
            consumer.Subscribe(Topics.CreateProduct);

            while (run)
            {
                var consumeResult = consumer.Consume();

                if (consumeResult == null)
                {
                    continue;
                }

                var message = JsonConvert.DeserializeObject<CreateProduct>(consumeResult.Message.Value);

                await _mediator.Send(new CreateProductCommand(message.Name, message.EventId));
            }

            consumer.Close();
        }

        public void EndConsumer()
        {
            run = false;
        }
    }
}
