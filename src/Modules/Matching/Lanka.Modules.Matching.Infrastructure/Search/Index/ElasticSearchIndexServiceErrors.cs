using Lanka.Common.Domain;

namespace Lanka.Modules.Matching.Infrastructure.Search.Index;

internal static class ElasticSearchIndexServiceErrors
{
    public static readonly Error IndexFailed = Error.Failure(
        "ElasticSearchIndex.IndexFailed",
        "Failed to index document in Elasticsearch"
    );

    public static readonly Error IndexError = Error.Failure(
        "ElasticSearchIndex.IndexError",
        "An error occurred while indexing document in Elasticsearch"
    );

    public static readonly Error BulkIndexFailed = Error.Failure(
        "ElasticSearchIndex.BulkIndexFailed",
        "Failed to perform bulk indexing in Elasticsearch"
    );

    public static readonly Error BulkIndexError = Error.Failure(
        "ElasticSearchIndex.BulkIndexError",
        "An error occurred during bulk indexing in Elasticsearch"
    );

    public static readonly Error RemoveFailed = Error.Failure(
        "ElasticSearchIndex.RemoveFailed",
        "Failed to remove document from Elasticsearch"
    );

    public static readonly Error RemoveError = Error.Failure(
        "ElasticSearchIndex.RemoveError",
        "An error occurred while removing document from Elasticsearch"
    );

    public static readonly Error RemoveBySourceEntityFailed = Error.Failure(
        "ElasticSearchIndex.RemoveBySourceEntityFailed",
        "Failed to remove documents by source entity from Elasticsearch"
    );

    public static readonly Error BulkRemoveFailed = Error.Failure(
        "ElasticSearchIndex.BulkRemoveFailed",
        "Failed to perform bulk removal in Elasticsearch"
    );

    public static readonly Error BulkRemoveError = Error.Failure(
        "ElasticSearchIndex.BulkRemoveError",
        "An error occurred during bulk removal in Elasticsearch"
    );

    public static readonly Error RefreshFailed = Error.Failure(
        "ElasticSearchIndex.RefreshFailed",
        "Failed to refresh Elasticsearch index"
    );

    public static readonly Error RefreshError = Error.Failure(
        "ElasticSearchIndex.RefreshError",
        "An error occurred while refreshing Elasticsearch index"
    );

    public static readonly Error ActivateBySourceEntityFailed = Error.Failure(
        "ElasticSearchIndex.ActivateBySourceEntityFailed",
        "Failed to activate documents by source entity in Elasticsearch"
    );

    public static readonly Error DeactivateBySourceEntityFailed = Error.Failure(
        "ElasticSearchIndex.DeactivateBySourceEntityFailed",
        "Failed to deactivate documents by source entity in Elasticsearch"
    );

    public static Error IndexErrorWithMessage(string message) => Error.Failure(
        "ElasticsearchIndex.IndexError",
        message);

    public static Error BulkIndexErrorWithMessage(string message) => Error.Failure(
        "ElasticSearchIndex.BulkIndexError",
        message
    );

    public static Error RemoveErrorWithMessage(string message) => Error.Failure(
        "ElasticSearchIndex.RemoveError",
        message
    );

    public static Error BulkRemoveErrorWithMessage(string message) => Error.Failure(
        "ElasticSearchIndex.BulkRemoveError",
        message
    );

    public static Error RefreshErrorWithMessage(string message) => Error.Failure(
        "ElasticSearchIndex.RefreshError",
        message
    );

    public static Error ActivateBySourceEntityFailedWithMessage(string message) => Error.Failure(
        "ElasticSearchIndex.ActivateBySourceEntityFailed",
        message
    );

    public static Error DeactivateBySourceEntityFailedWithMessage(string message) => Error.Failure(
        "ElasticSearchIndex.DeactivateBySourceEntityFailed",
        message
    );
}
