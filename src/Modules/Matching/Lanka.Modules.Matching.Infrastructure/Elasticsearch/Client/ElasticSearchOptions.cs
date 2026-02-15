namespace Lanka.Modules.Matching.Infrastructure.Elasticsearch.Client;

public sealed class ElasticSearchOptions
{
    public string DefaultIndex { get; set; } = "searchable_documents";

    public int PageSize { get; set; } = 10;

    public int MaxPageSize { get; set; } = 100;
}
