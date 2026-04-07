using System;
using System.Threading.Tasks;
using Ecommerce.Events.Audit;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Shared.Infrastructure.Audit
{
    public class AuditPublisher : IAuditPublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuditPublisher> _logger;
        private readonly string _serviceName;

        public AuditPublisher(
            IPublishEndpoint publishEndpoint,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuditPublisher> logger,
            string serviceName)
        {
            _publishEndpoint = publishEndpoint;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _serviceName = serviceName;
        }

        public async Task PublishAsync(string action, string entityType, string entityId,
            string actorId, string beforeState = "", string afterState = "",
            string ipAddress = "")
        {
            try
            {
                var correlationId = _httpContextAccessor.HttpContext?
                    .Request.Headers["X-Correlation-Id"].ToString() ?? Guid.NewGuid().ToString();
                if (string.IsNullOrEmpty(correlationId))
                    correlationId = Guid.NewGuid().ToString();

                var resolvedIp = ipAddress;
                if (string.IsNullOrEmpty(resolvedIp))
                    resolvedIp = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "";

                await _publishEndpoint.Publish(new AuditEntryCreated
                {
                    Service = _serviceName,
                    Action = action,
                    ActorId = actorId,
                    ActorType = string.IsNullOrEmpty(actorId) ? "System" : "User",
                    EntityType = entityType,
                    EntityId = entityId,
                    BeforeState = beforeState,
                    AfterState = afterState,
                    CorrelationId = correlationId,
                    IpAddress = resolvedIp,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to publish audit entry for {Action} on {EntityType}/{EntityId}",
                    action, entityType, entityId);
            }
        }
    }
}
