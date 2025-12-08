using Lanka.Modules.Campaigns.Domain.Bloggers.Categories;

namespace Lanka.Modules.Campaigns.Domain.Bloggers;

public interface IBloggerRepository
{
    Task<Blogger?> GetByIdAsync(
        BloggerId id,
        CancellationToken cancellationToken = default
    );

    void Add(Blogger blogger);

    void AttachCategory(Category category);
    
    void Remove(Blogger blogger);
}
