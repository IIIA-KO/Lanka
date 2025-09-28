using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Analysis;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Lanka.Modules.Matching.Infrastructure.Search.Documents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ExistsResponse = Elastic.Clients.Elasticsearch.IndexManagement.ExistsResponse;

namespace Lanka.Modules.Matching.Infrastructure.Search;

internal sealed class ElasticSearchInitializationService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ElasticSearchOptions _options;
    private readonly ILogger<ElasticSearchInitializationService> _logger;

    public ElasticSearchInitializationService(
        IServiceProvider serviceProvider,
        IOptions<ElasticSearchOptions> options,
        ILogger<ElasticSearchInitializationService> logger
    )
    {
        this._serviceProvider = serviceProvider;
        this._options = options.Value;
        this._logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = this._serviceProvider.CreateScope();
        ElasticsearchClient client = scope.ServiceProvider.GetRequiredService<ElasticsearchClient>();

        try
        {
            ElasticSearchOptions elasticOptions =
                scope.ServiceProvider.GetRequiredService<IOptions<ElasticSearchOptions>>().Value;
            
            this._logger.LogInformation(
                "Attempting to connect to Elasticsearch at: {BaseUrl}", 
                elasticOptions.BaseUrl
            );

            PingResponse pingResponse = await client.PingAsync(cancellationToken);

            if (!pingResponse.IsValidResponse)
            {
                this._logger.LogError(
                    "Cannot connect to Elasticsearch at {BaseUrl}. Response: {DebugInfo}",
                    elasticOptions.BaseUrl, pingResponse.DebugInformation
                );
                
                return;
            }

            this._logger.LogInformation("Successfully connected to Elasticsearch at {BaseUrl}", elasticOptions.BaseUrl);
            await this.CreateIndexIfNotExistsAsync(client, cancellationToken);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex,
                "Failed to initialize Elasticsearch index. Please check if Elasticsearch service is running and configuration is correct."
            );
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task CreateIndexIfNotExistsAsync(ElasticsearchClient client, CancellationToken cancellationToken)
    {
        ExistsResponse existsResponse =
            await client.Indices.ExistsAsync(this._options.DefaultIndex, cancellationToken);

        if (existsResponse.Exists)
        {
            this._logger.LogInformation("Elasticsearch index '{IndexName}' already exists",
                this._options.DefaultIndex
            );
            return;
        }

        this._logger.LogInformation(
            "Creating Elasticsearch index '{IndexName}'",
            this._options.DefaultIndex
        );

        CreateIndexResponse createIndexResponse = await client.Indices.CreateAsync(
            this._options.DefaultIndex, c => c
                .Settings(s => s
                    .NumberOfShards(1)
                    .NumberOfReplicas(0)
                    .Analysis(a => a
                        .Analyzers(analyzers => analyzers
                            .Custom("icu_analyzer", new CustomAnalyzer
                            {
                                Tokenizer = "icu_tokenizer",
                                Filter = ["lowercase", "icu_folding"]
                            })
                            .Custom("autocomplete_analyzer", new CustomAnalyzer
                            {
                                Tokenizer = "icu_tokenizer",
                                Filter = ["lowercase", "edge_ngram_filter"]
                            })
                        )
                        .TokenFilters(filters => filters
                            .EdgeNGram("edge_ngram_filter", new EdgeNGramTokenFilter
                            {
                                MinGram = 2,
                                MaxGram = 10
                            })
                        )
                    )
                )
                .Mappings(m => m
                    .Properties<SearchableDocumentElastic>(p => p
                        .Keyword(k => k.Id)
                        .Keyword(k => k.SourceEntityId)
                        .IntegerNumber(i => i.Type)
                        .Text(t => t.Title,
                            td => td
                                .Analyzer("icu_analyzer")
                                .Fields(f => f
                                    .Keyword(k => k.Title.Suffix("keyword"))
                                    .Text(tt => tt.Title.Suffix("autocomplete"), ttd => ttd
                                        .Analyzer("autocomplete_analyzer"))
                                )
                        )
                        .Text(t => t
                                .Content, td => td
                                .Analyzer("icu_analyzer")
                        )
                        .Text(t => t
                                .Tags, td => td
                                .Analyzer("icu_analyzer")
                                .Fields(f => f.Keyword(k => k.Tags.Suffix("keyword")))
                        )
                        .Object(o => o.Metadata)
                        .Boolean(b => b.IsActive)
                        .Date(d => d.LastUpdated)
                    )
                ), cancellationToken);

        if (!createIndexResponse.IsValidResponse)
        {
            this._logger.LogError("Failed to create Elasticsearch index: {Error}",
                createIndexResponse.DebugInformation
            );
            
            throw new InvalidOperationException(
                $"Failed to create Elasticsearch index: {createIndexResponse.DebugInformation}"
            );
        }

        this._logger.LogInformation("Successfully created Elasticsearch index '{IndexName}'",
            this._options.DefaultIndex
        );
    }
}
