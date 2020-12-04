using Ecommerce.Events;
using System.Threading.Tasks;

namespace Ecommerce.Shared.Infrastructure
{
    public interface IEventNotifier
    {
        Task Notify(IEvent content);
    }
}
