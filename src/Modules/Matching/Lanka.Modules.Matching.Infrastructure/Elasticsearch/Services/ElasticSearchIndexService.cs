using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Bulk;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Lanka.Modules.Matching.Application.Abstractions.Search;
using Lanka.Modules.Matching.Application.Index;
using Lanka.Modules.Matching.Domain.SearchableItems;
using Lanka.Modules.Matching.Infrastructure.Elasticsearch.Client;
using Lanka.Modules.Matching.Infrastructure.Elasticsearch.Documents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Result = Lanka.Common.Domain.Result;

namespace Lanka.Modules.Matching.Infrastructure.Elasticsearch.Services;

internal sealed class ElasticSearchIndexService : ISearchIndexService
{
    private readonly ElasticsearchClient _client;
    private readonly ILogger<ElasticSearchIndexService> _logger;
    private readonly ElasticSearchOptions _options;

    public ElasticSearchIndexService(
        ElasticsearchClient client,
        ILogger<ElasticSearchIndexService> logger,
        IOptions<ElasticSearchOptions> options
    )
    {
        this._client = client;
        this._logger = logger;
        this._options = options.Value;
    }

    public async Task<Result> IndexDocumentAsync(
        SearchDocument document,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            SearchableDocumentElastic elasticDoc = MapToElasticDocument(document);

            IndexResponse response = await this._client.IndexAsync(
                elasticDoc,
                this._options.DefaultIndex,
                elasticDoc.Id.ToString(),
                cancellationToken
            );

            if (!response.IsValidResponse)
            {
                this._logger.LogError("Failed to index document {DocumentId}: {Error}",
                    document.Id, response.DebugInformation
                );

                return Result.Failure(ElasticSearchIndexServiceErrors.IndexFailed);
            }

            this._logger.LogDebug("Successfully indexed document {DocumentId}", document.Id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error indexing document {DocumentId}", document.Id);
            return Result.Failure(ElasticSearchIndexServiceErrors.IndexErrorWithMessage(ex.Message));
        }
    }

    public async Task<Result> IndexDocumentsAsync(
        IEnumerable<SearchDocument> documents,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var documentList = documents.ToList();

            if (!documentList.Any())
            {
                return Result.Success();
            }

            var bulkRequest = new BulkRequest(this._options.DefaultIndex);
            var operations = documentList
                .Select(MapToElasticDocument)
                .Select(elasticDoc =>
                    new BulkIndexOperation<SearchableDocumentElastic>(elasticDoc)
                    {
                        Index = this._options.DefaultIndex,
                        Id = elasticDoc.Id.ToString()
                    }
                ).Cast<IBulkOperation>().ToList();

            bulkRequest.Operations = operations;

            BulkResponse response = await this._client.BulkAsync(bulkRequest, cancellationToken);

            if (!response.IsValidResponse || response.Errors)
            {
                IEnumerable<string> errors =
                    response.ItemsWithErrors.Select(item => item.Error?.Reason ?? "Unknown error");
                string errorMessage = string.Join("; ", errors);

                this._logger.LogError("Bulk indexing failed with errors: {Errors}", errorMessage);
                return Result.Failure(ElasticSearchIndexServiceErrors.BulkIndexFailed);
            }

            this._logger.LogDebug("Successfully bulk indexed {Count} documents", documentList.Count);
            return Result.Success();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error during bulk indexing");
            return Result.Failure(ElasticSearchIndexServiceErrors.BulkIndexErrorWithMessage(ex.Message));
        }
    }

    public async Task<Result> UpdateDocumentAsync(
        SearchDocument document,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            SearchableDocumentElastic elasticDoc = MapToElasticDocument(document);

            var updateRequest = new UpdateRequest<SearchableDocumentElastic, SearchableDocumentElastic>(
                this._options.DefaultIndex,
                elasticDoc.Id.ToString())
            {
                Doc = elasticDoc,
                DocAsUpsert = true
            };

            UpdateResponse<SearchableDocumentElastic> response = await this._client.UpdateAsync(
                updateRequest,
                cancellationToken
            );

            if (!response.IsValidResponse)
            {
                this._logger.LogError("Failed to update document {DocumentId}: {Error}",
                    document.Id, response.DebugInformation
                );

                return Result.Failure(ElasticSearchIndexServiceErrors.UpdateFailed);
            }

            this._logger.LogDebug("Successfully updated document {DocumentId}", document.Id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error updating document {DocumentId}", document.Id);
            return Result.Failure(ElasticSearchIndexServiceErrors.UpdateErrorWithMessage(ex.Message));
        }
    }

    public async Task<Result> RemoveDocumentAsync(
        Guid documentId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            DeleteResponse response = await this._client.DeleteAsync(
                new DeleteRequest(this._options.DefaultIndex, documentId.ToString()),
                cancellationToken
            );

            if (!response.IsValidResponse)
            {
                this._logger.LogError("Failed to remove document {DocumentId}: {Error}",
                    documentId, response.DebugInformation
                );

                return Result.Failure(ElasticSearchIndexServiceErrors.RemoveFailed);
            }

            this._logger.LogDebug("Successfully removed document {DocumentId}", documentId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error removing document {DocumentId}", documentId);
            return Result.Failure(ElasticSearchIndexServiceErrors.RemoveErrorWithMessage(ex.Message));
        }
    }

    public async Task<Result> RemoveDocumentsBySourceEntityAsync(
        Guid sourceEntityId,
        SearchableItemType type,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var deleteByQueryRequest = new DeleteByQueryRequest(this._options.DefaultIndex)
            {
                Query = new BoolQuery
                {
                    Must =
                    [
                        new TermQuery { Field = "sourceEntityId", Value = sourceEntityId.ToString() },
                        new TermQuery { Field = "type", Value = type.ToString() }
                    ]
                },
                Refresh = true
            };

            DeleteByQueryResponse response =
                await this._client.DeleteByQueryAsync(deleteByQueryRequest, cancellationToken);

            if (!response.IsValidResponse)
            {
                this._logger.LogError(
                    "Failed to remove documents by source entity {SourceEntityId} and type {Type}: {Error}",
                    sourceEntityId, type, response.DebugInformation
                );

                return Result.Failure(ElasticSearchIndexServiceErrors.RemoveBySourceEntityFailed);
            }

            this._logger.LogInformation(
                "Successfully removed {Count} documents for source entity {SourceEntityId} and type {Type}",
                response.Deleted, sourceEntityId, type
            );

            return Result.Success();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex,
                "Error removing documents by source entity {SourceEntityId} and type {Type} from Elasticsearch",
                sourceEntityId, type
            );

            return Result.Failure(ElasticSearchIndexServiceErrors.RemoveBySourceEntityFailed);
        }
    }

    public async Task<Result> RemoveDocumentsAsync(
        IEnumerable<Guid> documentIds,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var idList = documentIds.ToList();

            if (!idList.Any())
            {
                return Result.Success();
            }

            var bulkRequest = new BulkRequest(this._options.DefaultIndex);
            var operations = idList
                .Select(documentId =>
                    new BulkDeleteOperation(documentId.ToString())
                    {
                        Index = this._options.DefaultIndex
                    }
                ).Cast<IBulkOperation>().ToList();

            bulkRequest.Operations = operations;

            BulkResponse response = await this._client.BulkAsync(bulkRequest, cancellationToken);

            if (!response.IsValidResponse || response.Errors)
            {
                IEnumerable<string> errors =
                    response.ItemsWithErrors.Select(item => item.Error?.Reason ?? "Unknown error");
                string errorMessage = string.Join("; ", errors);

                this._logger.LogError("Bulk removal failed with errors: {Errors}", errorMessage);
                return Result.Failure(ElasticSearchIndexServiceErrors.BulkRemoveFailed);
            }

            this._logger.LogDebug("Successfully bulk removed {Count} documents", idList.Count);
            return Result.Success();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error during bulk removal");
            return Result.Failure(ElasticSearchIndexServiceErrors.BulkRemoveErrorWithMessage(ex.Message));
        }
    }

    public async Task<Result> RefreshIndexAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            Elastic.Clients.Elasticsearch.IndexManagement.RefreshResponse response =
                await this._client.Indices.RefreshAsync(this._options.DefaultIndex, cancellationToken);

            if (!response.IsValidResponse)
            {
                this._logger.LogError("Failed to refresh index: {Error}", response.DebugInformation);
                return Result.Failure(ElasticSearchIndexServiceErrors.RefreshFailed);
            }

            this._logger.LogDebug("Successfully refreshed index");
            return Result.Success();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error refreshing index");
            return Result.Failure(ElasticSearchIndexServiceErrors.RefreshErrorWithMessage(ex.Message));
        }
    }

    public async Task<bool> DocumentExistsAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        try
        {
            ExistsResponse response =
                await this._client.ExistsAsync(
                    new ExistsRequest(this._options.DefaultIndex, documentId.ToString()),
                    cancellationToken
                );
            return response.Exists;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error checking document existence {DocumentId}", documentId);
            return false;
        }
    }

    public async Task<Result> ActivateDocumentsBySourceEntityAsync(
        Guid sourceEntityId,
        SearchableItemType type,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var updateByQueryRequest = new UpdateByQueryRequest(this._options.DefaultIndex)
            {
                Query = new BoolQuery
                {
                    Must =
                    [
                        new TermQuery { Field = "sourceEntityId", Value = sourceEntityId.ToString() },
                        new TermQuery { Field = "type", Value = type.ToString() }
                    ]
                },
                Script = new Script
                {
                    Source = "ctx._source.isActive = true; ctx._source.lastUpdated = new Date().toISOString()"
                },
                Refresh = true
            };

            UpdateByQueryResponse response =
                await this._client.UpdateByQueryAsync(updateByQueryRequest, cancellationToken);

            if (!response.IsValidResponse)
            {
                this._logger.LogError(
                    "Failed to activate documents by source entity {SourceEntityId} and type {Type}: {Error}",
                    sourceEntityId, type, response.DebugInformation);

                return Result.Failure(ElasticSearchIndexServiceErrors.ActivateBySourceEntityFailed);
            }

            this._logger.LogInformation(
                "Successfully activated {Count} documents for source entity {SourceEntityId} and type {Type}",
                response.Updated, sourceEntityId, type
            );

            return Result.Success();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex,
                "Error activating documents by source entity {SourceEntityId} and type {Type} from Elasticsearch",
                sourceEntityId, type
            );

            return Result.Failure(ElasticSearchIndexServiceErrors.ActivateBySourceEntityFailed);
        }
    }

    public async Task<Result> DeactivateDocumentsBySourceEntityAsync(
        Guid sourceEntityId,
        SearchableItemType type,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var updateByQueryRequest = new UpdateByQueryRequest(this._options.DefaultIndex)
            {
                Query = new BoolQuery
                {
                    Must =
                    [
                        new TermQuery { Field = "sourceEntityId", Value = sourceEntityId.ToString() },
                        new TermQuery { Field = "type", Value = type.ToString() }
                    ]
                },
                Script = new Script
                {
                    Source = "ctx._source.isActive = false; ctx._source.lastUpdated = new Date().toISOString()"
                },
                Refresh = true
            };

            UpdateByQueryResponse response =
                await this._client.UpdateByQueryAsync(updateByQueryRequest, cancellationToken);

            if (!response.IsValidResponse)
            {
                this._logger.LogError(
                    "Failed to deactivate documents by source entity {SourceEntityId} and type {Type}: {Error}",
                    sourceEntityId, type, response.DebugInformation);

                return Result.Failure(ElasticSearchIndexServiceErrors.DeactivateBySourceEntityFailed);
            }

            this._logger.LogInformation(
                "Successfully deactivated {Count} documents for source entity {SourceEntityId} and type {Type}",
                response.Updated, sourceEntityId, type
            );

            return Result.Success();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex,
                "Error deactivating documents by source entity {SourceEntityId} and type {Type} from Elasticsearch",
                sourceEntityId, type
            );

            return Result.Failure(ElasticSearchIndexServiceErrors.DeactivateBySourceEntityFailed);
        }
    }

    public async Task<HashSet<Guid>> GetExistingSourceEntityIdsAsync(
        IEnumerable<Guid> sourceEntityIds,
        SearchableItemType type,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var idList = sourceEntityIds.ToList();
            if (idList.Count == 0)
            {
                return [];
            }

            // Query ES to find documents matching the given source entity IDs and type
            var searchRequest = new SearchRequest(this._options.DefaultIndex)
            {
                Size = idList.Count,
                Query = new BoolQuery
                {
                    Must =
                    [
                        new TermsQuery
                        {
                            Field = "sourceEntityId",
                            Terms = new TermsQueryField(idList.Select(id => FieldValue.String(id.ToString())).ToArray())
                        },
                        new TermQuery { Field = "type", Value = type.ToString() }
                    ]
                }
            };

            SearchResponse<SearchableDocumentElastic> response =
                await this._client.SearchAsync<SearchableDocumentElastic>(searchRequest, cancellationToken);

            if (!response.IsValidResponse)
            {
                this._logger.LogWarning("Failed to query existing documents: {Error}", response.DebugInformation);
                return [];
            }

            return response.Documents
                .Select(d => d.SourceEntityId)
                .ToHashSet();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error querying existing source entity IDs");
            return [];
        }
    }

    private static SearchableDocumentElastic MapToElasticDocument(SearchDocument document)
    {
        return new SearchableDocumentElastic
        {
            Id = document.Id,
            SourceEntityId = document.SourceEntityId,
            Type = document.Type.ToString(),
            Title = document.Title,
            Content = document.Content,
            Tags = document.Tags.ToList(),
            Metadata = document.Metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            IsActive = document.IsActive,
            LastUpdated = document.LastUpdated.DateTime
        };
    }
}
