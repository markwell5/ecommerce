using Ecommerce.Shared.Protos;
using GraphQL.Api.Types;

namespace GraphQL.Api.DataLoaders;

public class UserDataLoader : BatchDataLoader<string, User?>
{
    private readonly UserGrpc.UserGrpcClient _client;

    public UserDataLoader(
        UserGrpc.UserGrpcClient client,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _client = client;
    }

    protected override async Task<IReadOnlyDictionary<string, User?>> LoadBatchAsync(
        IReadOnlyList<string> keys,
        CancellationToken cancellationToken)
    {
        var results = new Dictionary<string, User?>();

        var tasks = keys.Select(async userId =>
        {
            try
            {
                var reply = await _client.GetUserAsync(
                    new GetUserRequest { UserId = userId },
                    cancellationToken: cancellationToken);
                return (userId, user: (User?)new User
                {
                    Id = reply.Id,
                    Email = reply.Email,
                    FirstName = reply.FirstName,
                    LastName = reply.LastName,
                    Phone = reply.Phone,
                    CreatedAt = reply.CreatedAt
                });
            }
            catch
            {
                return (userId, user: (User?)null);
            }
        });

        foreach (var result in await Task.WhenAll(tasks))
        {
            results[result.userId] = result.user;
        }

        return results;
    }
}
