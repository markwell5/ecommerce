using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Product.Application.Repositories;
using Product.Application.Domain;
using System.Linq;

namespace Product.Infrastructure
{
    public class MongoProductRepository : IProductRepository
    {
        IMongoCollection<MongoProductDto> products;
        public MongoProductRepository(MongoClient client)
        {
            products = client.GetDatabase("product-service")
                .GetCollection<MongoProductDto>("products");
        }

        public async Task<ProductDto> Create(ProductDto product)
        {
            var mp = new MongoProductDto { Name = product.Name, Key = await GetNextId() };
            await products.InsertOneAsync(mp);

            return mp;
        }

        public async Task<List<ProductDto>> Get()
        {
            return (await products.Find(_ => true).ToListAsync())
                .Cast<ProductDto>()
                .ToList();
        }

        public async Task<ProductDto> Get(long id)
        {
            return await products.Find(x => x.Key == id).FirstOrDefaultAsync();
        }

        private async Task<long> GetNextId()
        {
            return await products.CountDocumentsAsync(new BsonDocument()) + 1;
        }
    }

    public class MongoProductDto : ProductDto
    {
        [BsonId]
        public ObjectId MongoId { get; set; }
    }
}