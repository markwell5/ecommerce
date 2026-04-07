using System.Threading.Tasks;

namespace Ecommerce.Shared.Infrastructure.Audit
{
    public interface IAuditPublisher
    {
        Task PublishAsync(string action, string entityType, string entityId,
            string actorId, string beforeState = "", string afterState = "",
            string ipAddress = "");
    }
}
