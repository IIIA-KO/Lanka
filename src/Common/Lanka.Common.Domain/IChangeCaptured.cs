namespace Lanka.Common.Domain;

/// <summary>
/// Marker interface for entities whose changes are automatically captured
/// by the EF Core change capture interceptor and synced to Elasticsearch.
/// </summary>
public interface IChangeCaptured;
