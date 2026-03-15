using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Ecommerce.Shared.Infrastructure.Idempotency;

[AttributeUsage(AttributeTargets.Method)]
public class IdempotentEndpointAttribute : Attribute, IFilterMetadata
{
}
