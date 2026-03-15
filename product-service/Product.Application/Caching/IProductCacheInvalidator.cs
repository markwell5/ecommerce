using System.Threading;
using System.Threading.Tasks;

namespace Product.Application.Caching;

public interface IProductCacheInvalidator
{
    Task InvalidateProductAsync(long productId, CancellationToken cancellationToken = default);
    Task InvalidateAllAsync(CancellationToken cancellationToken = default);
}
