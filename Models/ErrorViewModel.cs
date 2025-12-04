namespace Ficto.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    // Content delivery error info
    public string? ErrorMessage { get; set; }
    public int? StatusCode { get; set; }
    public int? ErrorCode { get; set; }
    public string? ContentRequestUrl { get; set; }
    public bool HasContentError => !string.IsNullOrEmpty(ErrorMessage);
}
