using Lanka.Modules.Matching.Domain.SearchableItems;

namespace Lanka.Modules.Matching.UnitTests.SearchableItems;

internal static class SearchableItemData
{
    public static readonly Guid Id = Guid.Parse("87654321-4321-8765-cba9-876543210987");
    
    public const SearchableItemType Type = SearchableItemType.Campaign;
    
    public const string Title = "Test Searchable Item Title";
    
    public const string Content = "This is test content for the searchable item.";
    
    public static readonly IEnumerable<string> Tags = new[] { "test", "item", "search" };
    
    public const string UpdatedTitle = "Updated Searchable Item Title";
    
    public const string UpdatedContent = "This is updated content for the searchable item.";
    
    public static readonly IEnumerable<string> UpdatedTags = new[] { "updated", "modified", "item", "search", "test" };
}
