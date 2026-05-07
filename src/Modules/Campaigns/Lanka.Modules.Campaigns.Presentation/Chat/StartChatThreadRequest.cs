namespace Lanka.Modules.Campaigns.Presentation.Chat;

internal sealed record StartChatThreadRequest(
    Guid ParticipantBloggerId,
    Guid? OfferId = null,
    Guid? CampaignId = null);
