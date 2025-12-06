namespace Lanka.Modules.Analytics.Application.Abstractions.Models.Posts;

public class InstagramInsight
{
    public string Name { get; set; } = string.Empty;

    public string Period { get; set; } = "lifetime";

    public List<InstagramInsightValue> Values { get; set; } = [];
}

public class InstagramInsightValue
{
    public int? Value { get; set; }
}
