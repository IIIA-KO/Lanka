using FluentAssertions;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Data;
using Lanka.Modules.Analytics.Application.Abstractions.Instagram;
using Lanka.Modules.Analytics.Application.Abstractions.Models;
using Lanka.Modules.Analytics.Application.Abstractions.Models.Accounts;
using Lanka.Modules.Analytics.Application.Instagram.FetchAccountData;
using Lanka.Modules.Analytics.Application.UnitTests.Tokens;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;
using Lanka.Modules.Analytics.Domain.InstagramAccounts.Tokens;
using NSubstitute;

namespace Lanka.Modules.Analytics.Application.UnitTests.InstagramAccounts;

#pragma warning disable CA1515
public class FetchInstagramAccountDataTests
{
    private static FetchInstagramAccountDataCommand Command =>
        new(Guid.NewGuid(), "test_code");

    private readonly IInstagramTokenService _instagramTokenServiceMock;
    private readonly IInstagramAccountsService _instagramAccountsServiceMock;
    private readonly IInstagramAccountRepository _instagramAccountRepositoryMock;
    private readonly ITokenRepository _tokenRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;

    private readonly FetchInstagramAccountDataCommandHandler _handler;

    public FetchInstagramAccountDataTests()
    {
        this._instagramTokenServiceMock = Substitute.For<IInstagramTokenService>();
        this._instagramAccountsServiceMock = Substitute.For<IInstagramAccountsService>();
        this._instagramAccountRepositoryMock = Substitute.For<IInstagramAccountRepository>();
        this._tokenRepositoryMock = Substitute.For<ITokenRepository>();
        this._unitOfWorkMock = Substitute.For<IUnitOfWork>();

        this._handler = new FetchInstagramAccountDataCommandHandler(
            this._instagramTokenServiceMock,
            this._instagramAccountsServiceMock,
            this._instagramAccountRepositoryMock,
            this._tokenRepositoryMock,
            this._unitOfWorkMock
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess()
    {
        // Arrange
        this._instagramTokenServiceMock.GetAccessTokenAsync(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>()
        ).Returns(TokenData.FacebookTokenResponse);

        this._instagramAccountsServiceMock.GetUserInfoAsync(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>()
        ).Returns(InstagramAccountData.InstagramUserInfo);

        // Act
        Result result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTokenServiceFails()
    {
        // Arrange
        this._instagramTokenServiceMock.GetAccessTokenAsync(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>()
        ).Returns(Result.Failure<FacebookTokenResponse>(Error.NoData));

        // Act
        Result result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.NoData);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserInfoServiceFails()
    {
        // Arrange
        this._instagramTokenServiceMock.GetAccessTokenAsync(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>()
        ).Returns(TokenData.FacebookTokenResponse);

        this._instagramAccountsServiceMock.GetUserInfoAsync(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>()
        ).Returns(Result.Failure<InstagramUserInfo>(Error.NoData));

        // Act
        Result result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.NoData);
    }
}
