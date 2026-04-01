using HotChocolate.Authorization;

namespace GraphQL.Api.Types;

/// <summary>
/// Mutations forward to backend services via their REST APIs.
/// The GraphQL gateway is read-heavy by design — writes go through the existing
/// REST endpoints on each service which handle validation, events, and sagas.
/// This type acts as a placeholder for future write operations once gRPC write
/// endpoints are added to the backend services.
/// </summary>
[Authorize]
public class Mutation
{
    /// <summary>
    /// Placeholder mutation — the backend services currently expose write operations
    /// via REST only. Once gRPC write endpoints are added (createProduct, placeOrder, etc.),
    /// they will be wired here.
    /// </summary>
    public string Ping() => "pong";
}
