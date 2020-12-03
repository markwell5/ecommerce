using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Product.Infrastructure
{
    public interface IProductRepository
    {
        Task<List<ProductDto>> Get();
        Task<ProductDto> Create(string name);
        Task<ProductDto> GetProduct(long id);
    }

    public class MongoProductRepository : IProductRepository
    {
        IMongoCollection<ProductDto> products;
        public MongoProductRepository(MongoClient client)
        {
            products = client.GetDatabase("product-service")
                .GetCollection<ProductDto>("products");
        }

        public async Task<ProductDto> Create(string name)
        {
            var product = new ProductDto { Name = name, Id = await GetNextId() };
            await products.InsertOneAsync(product);

            return product;
        }

        public async Task<List<ProductDto>> Get()
        {
            return await products.Find(_ => true).ToListAsync();
        }

        public async Task<ProductDto> GetProduct(long id)
        {
            return await products.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        private async Task<long> GetNextId()
        {
            return await products.CountDocumentsAsync(new BsonDocument()) + 1;
        }
    }

    public class ProductDto
    {
        [BsonId]
        public ObjectId MongoId { get; set; }
        public long Id { get; set; }
        public string Name { get; set; }
    }
}