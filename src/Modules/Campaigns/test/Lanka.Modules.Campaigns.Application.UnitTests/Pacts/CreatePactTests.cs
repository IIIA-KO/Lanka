using FluentAssertions;
using Lanka.Common.Application.Authentication;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Application.Pacts.Create;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Pacts;
using NSubstitute;

namespace Lanka.Modules.Campaigns.Application.UnitTests.Pacts;

#pragma warning disable CA1515 // Type can be made internal
public class CreatePactTests
#pragma warning restore CA1515
{
    private static CreatePactCommand Command => new(PactData.Content);

    private readonly IPactRepository _pactRepositoryMock;
    private readonly IUserContext _userContextMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    
    private readonly CreatePactCommandHandler _handler;
    
    public CreatePactTests()
    {
        this._pactRepositoryMock = Substitute.For<IPactRepository>();
        this._userContextMock = Substitute.For<IUserContext>();
        this._unitOfWorkMock = Substitute.For<IUnitOfWork>();
        
        this._handler = new CreatePactCommandHandler(
            this._pactRepositoryMock,
            this._userContextMock,
            this._unitOfWorkMock
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnPactId()
    {
        // Arrange
        this._userContextMock.GetUserId()
            .Returns(Guid.NewGuid());

        this._pactRepositoryMock.GetByBloggerIdAsync(
            Arg.Any<BloggerId>(),
            Arg.Any<CancellationToken>()
        ).Returns((Pact?)null);
        
        
        // Act
        Result<Guid> result = await this._handler.Handle(
            Command,
            CancellationToken.None
        );
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task Handle_ShouldReturnDuplicateError_WhenPactAlreadyExists()
    {
        // Arrange
        this._userContextMock.GetUserId()
            .Returns(Guid.NewGuid());

        this._pactRepositoryMock.GetByBloggerIdAsync(
            Arg.Any<BloggerId>(),
            Arg.Any<CancellationToken>()
        ).Returns(PactData.CreatePact());
        
        // Act
        Result<Guid> result = await this._handler.Handle(
            Command,
            CancellationToken.None
        );
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PactErrors.Duplicate);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenPactCreationFails()
    {
        // Arrange
        this._userContextMock.GetUserId()
            .Returns(Guid.NewGuid());

        this._pactRepositoryMock.GetByBloggerIdAsync(
            Arg.Any<BloggerId>(),
            Arg.Any<CancellationToken>()
        ).Returns((Pact?)null);

        var command = new CreatePactCommand(string.Empty);
        
        // Act
        Result<Guid> result = await this._handler.Handle(
            command,
            CancellationToken.None
        );
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }
}
