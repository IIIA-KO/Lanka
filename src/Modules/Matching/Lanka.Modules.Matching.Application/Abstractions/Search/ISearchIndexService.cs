using Lanka.Common.Domain;
using Lanka.Modules.Matching.Domain.SearchableDocuments;
using Lanka.Modules.Matching.Domain.SearchableItems;

namespace Lanka.Modules.Matching.Application.Abstractions.Search;

public interface ISearchIndexService
{
    Task<Result> IndexDocumentAsync(SearchableDocument document, CancellationToken cancellationToken = default);
    
    Task<Result> IndexDocumentsAsync(IEnumerable<SearchableDocument> documents, CancellationToken cancellationToken = default);
    
    Task<Result> UpdateDocumentAsync(SearchableDocument document, CancellationToken cancellationToken = default);
    
    Task<Result> RemoveDocumentAsync(Guid documentId, CancellationToken cancellationToken = default);
    
    Task<Result> RemoveDocumentsBySourceEntityAsync(Guid sourceEntityId, SearchableItemType type, CancellationToken cancellationToken = default);
    
    Task<Result> RemoveDocumentsAsync(IEnumerable<Guid> documentIds, CancellationToken cancellationToken = default);
    
    Task<Result> ActivateDocumentsBySourceEntityAsync(Guid sourceEntityId, SearchableItemType type, CancellationToken cancellationToken = default);
    
    Task<Result> DeactivateDocumentsBySourceEntityAsync(Guid sourceEntityId, SearchableItemType type, CancellationToken cancellationToken = default);
}