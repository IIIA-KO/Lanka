using Lanka.Modules.Matching.Domain.SearchableItems;

namespace Lanka.Modules.Matching.UnitTests.SearchableDocuments;

internal static class SearchableDocumentData
{
    public static readonly Guid SourceEntityId = Guid.Parse("12345678-1234-5678-9abc-123456789012");
    
    public const SearchableItemType Type = SearchableItemType.Blogger;
    
    public const string Title = "Test Blog Post Title";
    
    public const string Content = "This is a test blog post content with meaningful text for search indexing.";
    
    public static readonly string LongTitle = new string('A', 600); // Exceeds max length of 500
    
    public static readonly string LongContent = new string('B', 15000); // Exceeds max length of 10000
    
    public static readonly IEnumerable<string> Tags = new[] { "technology", "programming", "testing" };
    
    public static readonly IDictionary<string, object> Metadata = new Dictionary<string, object>
    {
        ["category"] = "Technology",
        ["priority"] = 5,
        ["featured"] = true
    };
    
    public const string EmptyTitle = "";
    
    public const string EmptyContent = "";
    
    public const string WhitespaceTitle = "   ";
    
    public const string WhitespaceContent = "   ";
}
