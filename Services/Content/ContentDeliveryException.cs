using System.Net;
using Kontent.Ai.Delivery.Abstractions;

namespace Ficto.Services.Content;

/// <summary>
/// Thrown when the Kontent.ai Delivery API returns a server-side error (5xx).
/// Client-side errors (e.g. 404 not found) are handled gracefully via null/empty returns.
/// </summary>
public class ContentDeliveryException(HttpStatusCode statusCode, IError? error)
    : Exception(error?.Message ?? "Content delivery failed", error?.Exception)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
    public IError? Error { get; } = error;
}
