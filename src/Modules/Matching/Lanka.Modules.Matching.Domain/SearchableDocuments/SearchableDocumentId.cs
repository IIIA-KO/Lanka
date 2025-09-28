using Lanka.Common.Domain;

namespace Lanka.Modules.Matching.Domain.SearchableDocuments;

public sealed record SearchableDocumentId(Guid Value) : TypedEntityId(Value)
{
    public static SearchableDocumentId New() => new(Guid.NewGuid());
}
