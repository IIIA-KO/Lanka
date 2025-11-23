namespace Lanka.Modules.Matching.Infrastructure.Elasticsearch.Client;

public sealed class ElasticSearchOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    
    public string DefaultIndex { get; set; } = "searchable_documents";
    
    public string Username { get; set; } = string.Empty;
    
    public string Password { get; set; } = string.Empty;
    
    public int PageSize { get; set; } = 10;
    
    public int MaxPageSize { get; set; } = 100;
}
