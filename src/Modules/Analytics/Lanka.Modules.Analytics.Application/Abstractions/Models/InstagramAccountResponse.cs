using Lanka.Modules.Analytics.Domain.InstagramAccounts;

namespace Lanka.Modules.Analytics.Application.Abstractions.Models;

public class InstagramAccountResponse
{
    public Guid UserId { get; set; }
    
    public string FacebookPageId { get; set; }
    
    public string AdvertisementAccountId { get; set; }
    
    public Metadata Metadata { get; set; }
}
