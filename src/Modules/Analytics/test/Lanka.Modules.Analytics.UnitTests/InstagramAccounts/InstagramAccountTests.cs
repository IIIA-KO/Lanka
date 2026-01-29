using FluentAssertions;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;
using Lanka.Modules.Analytics.Domain.InstagramAccounts.DomainEvents;
using Lanka.Modules.Analytics.Domain.InstagramAccounts.Metadatas;
using Lanka.Modules.Analytics.UnitTests.Abstractions;

namespace Lanka.Modules.Analytics.UnitTests.InstagramAccounts;

#pragma warning disable CA1515
public class InstagramAccountTests : BaseTest // Consider making public types internal
{
    [Fact]
    public void Create_ShouldReturnInstagramAccount()
    {
        // Act
        Result<InstagramAccount> instagramAccount = InstagramAccount.Create(
            InstagramAccountData.UserId,
            InstagramAccountData.Email,
            InstagramAccountData.FacebookPageId,
            InstagramAccountData.AdvertisementAccountId,
            InstagramAccountData.BusinessDiscoveryId,
            InstagramAccountData.BusinessDiscoveryIgId,
            InstagramAccountData.BusinessDiscoveryUsername,
            InstagramAccountData.BusinessDiscoveryFollowersCount,
            InstagramAccountData.BusinessDiscoveryMediaCount
        );

        // Assert
        instagramAccount.Should().NotBeNull();
        instagramAccount.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenFacebookPageIdIsInvalid()
    {
        // Act
        Result<InstagramAccount> result = InstagramAccount.Create(
            InstagramAccountData.UserId,
            InstagramAccountData.Email,
            " ",
            InstagramAccountData.AdvertisementAccountId,
            InstagramAccountData.BusinessDiscoveryId,
            InstagramAccountData.BusinessDiscoveryIgId,
            InstagramAccountData.BusinessDiscoveryUsername,
            InstagramAccountData.BusinessDiscoveryFollowersCount,
            InstagramAccountData.BusinessDiscoveryMediaCount
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenAdvertisementAccountIdIsInvalid()
    {
        // Act
        Result<InstagramAccount> result = InstagramAccount.Create(
            InstagramAccountData.UserId,
            InstagramAccountData.Email,
            InstagramAccountData.FacebookPageId,
            null!,
            InstagramAccountData.BusinessDiscoveryId,
            InstagramAccountData.BusinessDiscoveryIgId,
            InstagramAccountData.BusinessDiscoveryUsername,
            InstagramAccountData.BusinessDiscoveryFollowersCount,
            InstagramAccountData.BusinessDiscoveryMediaCount
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Create_ShouldRaiseDomainEvent_WhenInstagramAccountCreated()
    {
        // Arrange
        Result<InstagramAccount> instagramAccountResult = InstagramAccount.Create(
            InstagramAccountData.UserId,
            InstagramAccountData.Email,
            InstagramAccountData.FacebookPageId,
            InstagramAccountData.AdvertisementAccountId,
            InstagramAccountData.BusinessDiscoveryId,
            InstagramAccountData.BusinessDiscoveryIgId,
            InstagramAccountData.BusinessDiscoveryUsername,
            InstagramAccountData.BusinessDiscoveryFollowersCount,
            InstagramAccountData.BusinessDiscoveryMediaCount
        );

        // Act
        InstagramAccount instagramAccount = instagramAccountResult.Value;

        // Assert
        AssertDomainEventWasPublished<InstagramAccountDataFetchedDomainEvent>(instagramAccount);
    }

    [Fact]
    public void Update_ShouldUpdateInstagramAccountMetadata()
    {
        // Arrange
        Result<InstagramAccount> instagramAccountResult = InstagramAccount.Create(
            InstagramAccountData.UserId,
            InstagramAccountData.Email,
            InstagramAccountData.FacebookPageId,
            InstagramAccountData.AdvertisementAccountId,
            InstagramAccountData.BusinessDiscoveryId,
            InstagramAccountData.BusinessDiscoveryIgId,
            InstagramAccountData.BusinessDiscoveryUsername,
            InstagramAccountData.BusinessDiscoveryFollowersCount,
            InstagramAccountData.BusinessDiscoveryMediaCount
        );

        InstagramAccount instagramAccount = instagramAccountResult.Value;

        Metadata updatedMetadata = Metadata.Create("new-id", 12345, "new-username", 100, 100).Value;
        // Act
        instagramAccount.Update(updatedMetadata);

        // Assert
        instagramAccount.Metadata.Should().Be(updatedMetadata);
    }
}
