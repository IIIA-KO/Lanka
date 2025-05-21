namespace Lanka.Modules.Campaigns.Domain.Bloggers;

public interface IBloggerRepository
{
    Task<Blogger?> GetByIdAsync(
        BloggerId id,
        CancellationToken cancellationToken = default
    );

    void Add(Blogger blogger);
    
    void Remove(Blogger blogger);
}
