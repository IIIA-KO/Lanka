namespace Lanka.Modules.Users.Application.Instagram.Models;

public sealed record InstagramOperationStatus(
    string OperationType, // "linking" or "renewal"
    string Status, // "not_found", "pending", "processing", "completed", "failed"
    string? Message = null,
    DateTime? StartedAt = null,
    DateTime? CompletedAt = null
);

public static class InstagramOperationType
{
    public const string Linking = "linking";
    public const string Renewal = "renewal";
}

public static class InstagramOperationStatuses
{
    public const string NotFound = "not_found";
    public const string Pending = "pending";
    public const string Processing = "processing";
    public const string Completed = "completed";
    public const string Failed = "failed";
}
