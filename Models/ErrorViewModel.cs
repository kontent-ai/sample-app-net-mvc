namespace Ficto.Models;

public record ErrorViewModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    public int? StatusCode { get; set; }
}
