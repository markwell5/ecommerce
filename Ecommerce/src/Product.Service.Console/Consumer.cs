using Confluent.Kafka;
using Core.Domain;
using Core.Domain.Commands;
using MediatR;
using Newtonsoft.Json;
using Product.Service.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Product.Service
{
    public class Consumer
    {
        private readonly IMediator _mediator;

        public Consumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task Consume()
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = "localhost:9092",
                GroupId = "product-service",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe(Topics.CreateProduct);

            while (true)
            {
                var consumeResult = consumer.Consume(2);

                if(consumeResult == null)
                {
                    continue;
                }

                var message = JsonConvert.DeserializeObject<CreateProduct>(consumeResult.Message.Value);

                _mediator.Send(new CreateProductCommand(message.Name));
            }

            consumer.Close();
        }
    }
}
