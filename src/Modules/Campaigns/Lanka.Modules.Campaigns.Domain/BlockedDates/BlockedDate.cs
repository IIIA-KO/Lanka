using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Domain.Bloggers;

namespace Lanka.Modules.Campaigns.Domain.BlockedDates;

public sealed class BlockedDate : Entity<BlockedDateId>
{
    private BlockedDate() { }

    public BlockedDate(BloggerId bloggerId, DateOnly date)
        : base(BlockedDateId.New())
    {
        this.BloggerId = bloggerId;
        this.Date = date;
    }

    public BloggerId BloggerId { get; init; }

    public Blogger? Blogger { get; init; }

    public DateOnly Date { get; private set; }
}
