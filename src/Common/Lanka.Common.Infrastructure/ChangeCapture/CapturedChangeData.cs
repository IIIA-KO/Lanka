namespace Lanka.Common.Infrastructure.ChangeCapture;

public sealed record CapturedChangeData(
    string ItemType,
    string Title,
    string Content,
    IReadOnlyCollection<string> Tags,
    IDictionary<string, object>? Metadata = null
);
