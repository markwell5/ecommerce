using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Audit.Application.Entities;
using Ecommerce.Events.Audit;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Audit.Application.Consumers
{
    public class AuditEntryConsumer : IConsumer<AuditEntryCreated>
    {
        private readonly AuditDbContext _dbContext;
        private readonly ILogger<AuditEntryConsumer> _logger;

        public AuditEntryConsumer(AuditDbContext dbContext, ILogger<AuditEntryConsumer> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<AuditEntryCreated> context)
        {
            var msg = context.Message;

            var previousHash = await _dbContext.AuditEntries
                .OrderByDescending(e => e.Id)
                .Select(e => e.Hash)
                .FirstOrDefaultAsync() ?? "GENESIS";

            var entry = new AuditEntry
            {
                Service = msg.Service,
                Action = msg.Action,
                ActorId = msg.ActorId,
                ActorType = msg.ActorType,
                EntityType = msg.EntityType,
                EntityId = msg.EntityId,
                BeforeState = msg.BeforeState,
                AfterState = msg.AfterState,
                CorrelationId = msg.CorrelationId,
                IpAddress = msg.IpAddress,
                Timestamp = msg.Timestamp,
                PreviousHash = previousHash
            };

            entry.Hash = ComputeHash(entry, previousHash);

            _dbContext.AuditEntries.Add(entry);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Audit: {Service}.{Action} on {EntityType}/{EntityId} by {ActorId}",
                msg.Service, msg.Action, msg.EntityType, msg.EntityId, msg.ActorId);
        }

        private static string ComputeHash(AuditEntry entry, string previousHash)
        {
            var data = $"{previousHash}|{entry.Timestamp:O}|{entry.Service}|{entry.Action}|{entry.ActorId}|{entry.EntityType}|{entry.EntityId}";
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
