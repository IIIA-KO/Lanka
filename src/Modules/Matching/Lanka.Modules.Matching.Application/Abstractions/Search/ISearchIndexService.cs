using Lanka.Common.Domain;
using Lanka.Modules.Matching.Application.Index;
using Lanka.Modules.Matching.Domain.SearchableItems;

namespace Lanka.Modules.Matching.Application.Abstractions.Search;

public interface ISearchIndexService
{
    Task<Result> IndexDocumentAsync(SearchDocument document, CancellationToken cancellationToken = default);
    
    Task<Result> IndexDocumentsAsync(IEnumerable<SearchDocument> documents, CancellationToken cancellationToken = default);
    
    Task<Result> UpdateDocumentAsync(SearchDocument document, CancellationToken cancellationToken = default);
    
    Task<Result> RemoveDocumentAsync(Guid documentId, CancellationToken cancellationToken = default);
    
    Task<Result> RemoveDocumentsBySourceEntityAsync(Guid sourceEntityId, SearchableItemType type, CancellationToken cancellationToken = default);
    
    Task<Result> RemoveDocumentsAsync(IEnumerable<Guid> documentIds, CancellationToken cancellationToken = default);
    
    Task<Result> ActivateDocumentsBySourceEntityAsync(Guid sourceEntityId, SearchableItemType type, CancellationToken cancellationToken = default);
    
    Task<Result> DeactivateDocumentsBySourceEntityAsync(Guid sourceEntityId, SearchableItemType type, CancellationToken cancellationToken = default);

    Task<HashSet<Guid>> GetExistingSourceEntityIdsAsync(IEnumerable<Guid> sourceEntityIds, SearchableItemType type, CancellationToken cancellationToken = default);
}