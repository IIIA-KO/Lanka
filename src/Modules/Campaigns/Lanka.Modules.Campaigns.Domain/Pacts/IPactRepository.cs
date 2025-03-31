using Lanka.Modules.Campaigns.Domain.Bloggers;

namespace Lanka.Modules.Campaigns.Domain.Pacts;

public interface IPactRepository
{
    Task<Pact?> GetByIdAsync(PactId id, CancellationToken cancellationToken = default);
    
    Task<Pact?> GetByBloggerIdAsync(BloggerId bloggerId, CancellationToken cancellationToken = default);
    
    Task<Pact?> GetByIdWithOffersAsync(PactId id, CancellationToken cancellationToken = default);
    
    Task<Pact?> GetByBloggerIdWithOffersAsync(BloggerId bloggerId, CancellationToken cancellationToken = default);
    
    void Add(Pact pact);
}
