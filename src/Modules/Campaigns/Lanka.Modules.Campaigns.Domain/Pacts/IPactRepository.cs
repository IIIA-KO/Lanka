namespace Lanka.Modules.Campaigns.Domain.Pacts;

public interface IPactRepository
{
    Task<Pact?> GetByIdWithOffersAsync(PactId id, CancellationToken cancellationToken = default);
}