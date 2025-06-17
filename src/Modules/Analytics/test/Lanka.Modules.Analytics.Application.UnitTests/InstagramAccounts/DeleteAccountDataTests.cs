using FluentAssertions;
using Lanka.Common.Domain;
using Lanka.Modules.Analytics.Application.Abstractions.Data;
using Lanka.Modules.Analytics.Application.Instagram.DeleteAccountData;
using Lanka.Modules.Analytics.Domain.InstagramAccounts;
using NSubstitute;

namespace Lanka.Modules.Analytics.Application.UnitTests.InstagramAccounts;

#pragma warning disable CA1515
public class DeleteAccountDataTests
{
    private static DeleteInstagramAccountDataCommand Command => new(Guid.NewGuid());

    private readonly IInstagramAccountRepository _instagramAccountRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;

    private readonly DeleteInstagramAccountDataCommandHandler _handler;

    public DeleteAccountDataTests()
    {
        this._instagramAccountRepositoryMock = Substitute.For<IInstagramAccountRepository>();
        this._unitOfWorkMock = Substitute.For<IUnitOfWork>();

        this._handler = new DeleteInstagramAccountDataCommandHandler(
            this._instagramAccountRepositoryMock,
            this._unitOfWorkMock
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess()
    {
        // Arrange
        this._instagramAccountRepositoryMock.GetByUserIdWithTokenAsync(
            Arg.Any<UserId>(),
            Arg.Any<CancellationToken>()
        ).Returns(InstagramAccountData.Create());

        // Act
        Result result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
    
    [Fact]
    public async Task Handle_ShouldRemoveInstagramAccount()
    {
        // Arrange
        InstagramAccount instagramAccount = InstagramAccountData.Create();
        this._instagramAccountRepositoryMock.GetByUserIdWithTokenAsync(
            Arg.Any<UserId>(),
            Arg.Any<CancellationToken>()
        ).Returns(instagramAccount);

        // Act
        await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        this._instagramAccountRepositoryMock.Received(1).Remove(instagramAccount);
        await this._unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenInstagramAccountDoesNotExist()
    {
        // Arrange
        this._instagramAccountRepositoryMock.GetByUserIdWithTokenAsync(
            Arg.Any<UserId>(),
            Arg.Any<CancellationToken>()
        ).Returns((InstagramAccount?)null);

        // Act
        Result result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(InstagramAccountErrors.NotFound);
    }
}
