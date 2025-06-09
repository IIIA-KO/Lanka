namespace Lanka.Modules.Analytics.Application.Abstractions.Models.Posts;

public class InstagramPagingResponse
{
    public string? Before { get; set; }

    public string? After { get; set; }

    public string? NextCursor { get; set; }

    public string? PreviousCursor { get; set; }
}
