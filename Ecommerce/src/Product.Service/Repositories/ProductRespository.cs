using Dapper;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using Product.Service.Domain;
using System.Threading.Tasks;

namespace Product.Service.Repositories
{
    public interface IProductRespository
    {
        Task<int> Create(ProductDto product);
    }

    public class ProductRespository : IProductRespository
    {
        private readonly string _connectionString;

        public ProductRespository(IOptions<Config.Settings> settings)
        {
            _connectionString = settings.Value.ConnectionStrings.DbConnectionString;
        }

        public async Task<int> Create(ProductDto product)
        {
            using var con = new MySqlConnection(_connectionString);

            return await con.QuerySingleOrDefaultAsync<int>(
                "INSERT INTO Product (Name) VALUES (@name); SELECT LAST_INSERT_ID();",
                new { name = product.Name });
        }
    }
}
