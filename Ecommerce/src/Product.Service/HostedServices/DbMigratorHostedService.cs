using Dapper;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using Product.Service.Config;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace Product.Service.HostedServices
{
    public class DbMigratorHostedService : IHostedService
    {
        private readonly Settings _settings;

        public DbMigratorHostedService(IOptions<Settings> settings)
        {
            _settings = settings.Value;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var con = new MySqlConnection(_settings.ConnectionStrings.DbConnectionString);

            await con.QueryAsync(@"CREATE TABLE IF NOT EXISTS Product (
                Id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                Name VARCHAR(50) NOT NULL
            );");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
