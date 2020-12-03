using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Model.Product.Request;
using Ecommerce.Model.Product.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Product.Infrastructure;

namespace Product.Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ILogger<ProductController> _logger;
        private readonly IProductRepository _repo;

        public ProductController(ILogger<ProductController> logger, IProductRepository repo)
        {
            _logger = logger;
            _repo = repo;
        }

        [HttpGet]
        public async Task<IEnumerable<ProductResponse>> GetProducts()
        {
            return (await _repo.Get()).Select(d => new ProductResponse { Id = d.Id, Name = d.Name });
        }

        [HttpGet("{id}")]
        public async Task<ProductResponse> GetProduct(long id)
        {
            var d = await _repo.GetProduct(id);

            return new ProductResponse
            {
                Id = d.Id,
                Name = d.Name
            };
        }

        [HttpPost]
        public async Task<ProductResponse> Create([FromBody] CreateProductRequest req)
        {
            var d = await _repo.Create(req.Name);

            return new ProductResponse
            {
                Id = d.Id,
                Name = d.Name
            };
        }
    }
}
