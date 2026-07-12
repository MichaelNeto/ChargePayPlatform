using System.Text.Json.Serialization;

namespace ChargePay.Application.Common;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ResponseCode
{
    SUCCESS,
    VALIDATION_ERROR,
    BUSINESS_ERROR,
    NOT_FOUND,
    CONFLICT,
    UNAUTHORIZED,
    FORBIDDEN,
    INTERNAL_ERROR
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ErrorType
{
    Validation,
    Business,
    Authentication,
    Authorization,
    Conflict,
    NotFound,
    Infrastructure,
    Unexpected
}

public sealed class ApiMetadata
{
    public DateTime Timestamp { get; set; }
    public string TraceId { get; set; } = null!;
    public string CorrelationId { get; set; } = null!;
    public string ApiVersion { get; set; } = null!;
}

public sealed class ErrorDetail
{
    public string Field { get; set; } = null!;
    public string Code { get; set; } = null!;
    public ErrorType Type { get; set; }
    public string Message { get; set; } = null!;
}

public sealed class ApiResponse<T>
{
    public bool Success { get; set; }
    public ResponseCode Code { get; set; }
    public string Message { get; set; } = null!;
    public T? Data { get; set; }
    public List<ErrorDetail> Errors { get; set; } = new();
    public ApiMetadata Metadata { get; set; } = null!;

    public static ApiResponse<T> SuccessResponse(T data, string message, ApiMetadata metadata)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Code = ResponseCode.SUCCESS,
            Message = message,
            Data = data,
            Errors = new List<ErrorDetail>(),
            Metadata = metadata
        };
    }

    public static ApiResponse<T> Failure(ResponseCode code, string message, List<ErrorDetail> errors, ApiMetadata metadata)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Code = code,
            Message = message,
            Data = default,
            Errors = errors,
            Metadata = metadata
        };
    }
}
