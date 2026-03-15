using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Ecommerce.Shared.Infrastructure.Validation;

public class InputSanitizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        SanitizeStrings(request);
        return next();
    }

    private static void SanitizeStrings(object obj)
    {
        if (obj == null) return;

        var type = obj.GetType();

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!property.CanRead || !property.CanWrite)
                continue;

            if (property.PropertyType == typeof(string))
            {
                var value = (string)property.GetValue(obj);
                if (value != null)
                {
                    var sanitized = WebUtility.HtmlEncode(value.Trim());
                    property.SetValue(obj, sanitized);
                }
            }
            else if (property.PropertyType.IsClass
                     && property.PropertyType != typeof(string)
                     && !property.PropertyType.IsArray
                     && property.PropertyType.Namespace != null
                     && !property.PropertyType.Namespace.StartsWith("System"))
            {
                var nested = property.GetValue(obj);
                if (nested != null)
                    SanitizeStrings(nested);
            }
        }
    }
}
