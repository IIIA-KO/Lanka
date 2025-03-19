using Lanka.Modules.Campaigns.Domain.Bloggers;

namespace Lanka.Modules.Campaigns.Domain.BlockedDates
{
    public interface IBlockedDateRepository
    {
        Task<BlockedDate?> GetByIdAsync(
            BlockedDateId id,
            CancellationToken cancellationToken = default
        );
        
        public Task<BlockedDate?> GetByDateAndBloggerIdAsync(
            DateOnly date, 
            BloggerId bloggerId, 
            CancellationToken cancellationToken = default
        );
        
        void Add(BlockedDate blockedDate);
        
        void Remove(BlockedDate blockedDate);
    }
}
