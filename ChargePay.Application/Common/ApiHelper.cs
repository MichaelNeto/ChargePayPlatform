using Microsoft.AspNetCore.Http;

namespace ChargePay.Application.Common;

public static class ApiHelper
{
    public static ApiMetadata CreateMetadata(HttpContext httpContext)
    {
        return new ApiMetadata
        {
            Timestamp = DateTime.UtcNow,
            TraceId = httpContext.TraceIdentifier,
            CorrelationId = httpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? Guid.NewGuid().ToString(),
            ApiVersion = "v1"
        };
    }
}
