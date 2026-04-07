using System;
using System.Threading.Tasks;
using Analytics.Application.Entities;
using Ecommerce.Events.User;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.Consumers
{
    public class UserRegisteredConsumer : IConsumer<UserRegistered>
    {
        private readonly AnalyticsDbContext _dbContext;
        private readonly ILogger<UserRegisteredConsumer> _logger;

        public UserRegisteredConsumer(AnalyticsDbContext dbContext, ILogger<UserRegisteredConsumer> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserRegistered> context)
        {
            var msg = context.Message;
            var exists = await _dbContext.CustomerRecords.AnyAsync(c => c.UserId == msg.UserId);
            if (exists) return;

            _dbContext.CustomerRecords.Add(new CustomerRecord
            {
                UserId = msg.UserId,
                Email = msg.Email,
                RegisteredAt = DateTime.UtcNow
            });

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Analytics: Customer {UserId} registered", msg.UserId);
        }
    }
}
