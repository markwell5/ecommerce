using Product.Application.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Product.Application.Repositories
{
    public interface IProductRepository
    {
        Task<List<ProductDto>> Get();
        Task<ProductDto> Create(ProductDto product);
        Task<ProductDto> Get(long id);
    }
}
