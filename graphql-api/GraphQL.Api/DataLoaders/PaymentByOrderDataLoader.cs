using Ecommerce.Shared.Protos;
using GraphQL.Api.Types;

namespace GraphQL.Api.DataLoaders;

public class PaymentByOrderDataLoader : BatchDataLoader<string, Payment?>
{
    private readonly PaymentGrpc.PaymentGrpcClient _client;

    public PaymentByOrderDataLoader(
        PaymentGrpc.PaymentGrpcClient client,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _client = client;
    }

    protected override async Task<IReadOnlyDictionary<string, Payment?>> LoadBatchAsync(
        IReadOnlyList<string> keys,
        CancellationToken cancellationToken)
    {
        var results = new Dictionary<string, Payment?>();

        var tasks = keys.Select(async orderId =>
        {
            try
            {
                var reply = await _client.GetPaymentByOrderAsync(
                    new GetPaymentByOrderRequest { OrderId = orderId },
                    cancellationToken: cancellationToken);
                return (orderId, payment: (Payment?)new Payment
                {
                    Id = reply.Id,
                    OrderId = reply.OrderId,
                    CustomerId = reply.CustomerId,
                    Amount = decimal.TryParse(reply.Amount, out var a) ? a : 0,
                    Currency = reply.Currency,
                    Status = reply.Status,
                    StripePaymentIntentId = reply.StripePaymentIntentId,
                    CreatedAt = reply.CreatedAt
                });
            }
            catch
            {
                return (orderId, payment: (Payment?)null);
            }
        });

        foreach (var result in await Task.WhenAll(tasks))
        {
            results[result.orderId] = result.payment;
        }

        return results;
    }
}
