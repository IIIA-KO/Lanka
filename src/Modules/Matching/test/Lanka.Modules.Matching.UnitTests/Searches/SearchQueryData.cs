using Lanka.Modules.Matching.Domain.SearchableItems;

namespace Lanka.Modules.Matching.UnitTests.Searches;

internal static class SearchQueryData
{
    public const string Text = "test search query";
    
    public const string EmptyText = "";
    
    public const string WhitespaceText = "   ";
    
    public static readonly string LongText = new string('X', 2000);
    
    public const double ValidFuzzyDistance = 0.7;
    
    public const double InvalidFuzzyDistanceLow = -0.5;
    
    public const double InvalidFuzzyDistanceHigh = 1.5;
    
    public const int ValidPage = 2;
    
    public const int InvalidPage = -1;
    
    public const int ValidSize = 25;
    
    public const int InvalidSize = -10;
    
    public const int OversizedSize = 1500;
    
    public static readonly DateTimeOffset CreatedAfter = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
    
    public static readonly DateTimeOffset CreatedBefore = new DateTimeOffset(2024, 12, 31, 23, 59, 59, TimeSpan.Zero);
    
    public const string ItemTypes = "1,2,3";
    
    public static readonly IDictionary<string, object> NumericFilters = new Dictionary<string, object>
    {
        ["minPrice"] = 50.0,
        ["maxFollowers"] = 5000
    };
    
    public static readonly IDictionary<string, IReadOnlyCollection<string>> FacetFilters = 
        new Dictionary<string, IReadOnlyCollection<string>>
        {
            ["categories"] = new[] { "Technology", "Programming", "Testing" },
            ["regions"] = new[] { "North America", "Europe" }
        };
}
