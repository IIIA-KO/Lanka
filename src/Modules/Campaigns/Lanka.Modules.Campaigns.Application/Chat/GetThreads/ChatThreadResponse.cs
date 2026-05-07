namespace Lanka.Modules.Campaigns.Application.Chat.GetThreads;

public sealed record ChatThreadResponse(
    Guid Id,
    Guid OtherParticipantId,
    string OtherParticipantFirstName,
    string OtherParticipantLastName,
    string? OtherParticipantInstagramUsername,
    string? OtherParticipantProfilePhoto,
    Guid? CampaignId,
    string? CampaignName,
    Guid? OfferId,
    string? OfferName,
    string? LastMessageContent,
    bool LastMessageIsSystem,
    DateTime? LastMessageCreatedAtUtc,
    int UnreadCount,
    DateTime UpdatedAtUtc);
