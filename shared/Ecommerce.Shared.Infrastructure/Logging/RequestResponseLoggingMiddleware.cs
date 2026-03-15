using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ecommerce.Shared.Infrastructure.Logging;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RequestResponseLoggingSettings _settings;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(
        RequestDelegate next,
        IOptions<RequestResponseLoggingSettings> settings,
        ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_settings.Enabled)
        {
            await _next(context);
            return;
        }

        await LogRequest(context);

        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        await LogResponse(context, responseBody, originalBodyStream);
    }

    private async Task LogRequest(HttpContext context)
    {
        context.Request.EnableBuffering();

        var body = string.Empty;
        if (context.Request.ContentLength > 0)
        {
            using var reader = new StreamReader(
                context.Request.Body,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true);
            body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        var maskedBody = MaskSensitiveData(Truncate(body));

        _logger.LogDebug(
            "HTTP {Method} {Path}{QueryString} - Body: {RequestBody}",
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString,
            maskedBody);
    }

    private async Task LogResponse(HttpContext context, MemoryStream responseBody, Stream originalBodyStream)
    {
        responseBody.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(responseBody, Encoding.UTF8).ReadToEndAsync();
        responseBody.Seek(0, SeekOrigin.Begin);

        await responseBody.CopyToAsync(originalBodyStream);
        context.Response.Body = originalBodyStream;

        var maskedBody = MaskSensitiveData(Truncate(body));

        _logger.LogDebug(
            "HTTP {StatusCode} {Method} {Path} - Body: {ResponseBody}",
            context.Response.StatusCode,
            context.Request.Method,
            context.Request.Path,
            maskedBody);
    }

    private string Truncate(string value)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= _settings.MaxBodyLength)
            return value;

        return value[.._settings.MaxBodyLength] + "...[truncated]";
    }

    private string MaskSensitiveData(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return body;

        try
        {
            var node = JsonNode.Parse(body);
            if (node != null)
            {
                MaskNode(node);
                return node.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
            }
        }
        catch (JsonException)
        {
            // Not JSON — return as-is
        }

        return body;
    }

    private void MaskNode(JsonNode node)
    {
        if (node is JsonObject obj)
        {
            foreach (var property in obj.ToArray())
            {
                if (_settings.SensitiveFields.Contains(property.Key, StringComparer.OrdinalIgnoreCase))
                {
                    obj[property.Key] = "***MASKED***";
                }
                else if (property.Value != null)
                {
                    MaskNode(property.Value);
                }
            }
        }
        else if (node is JsonArray array)
        {
            foreach (var item in array)
            {
                if (item != null)
                    MaskNode(item);
            }
        }
    }
}
