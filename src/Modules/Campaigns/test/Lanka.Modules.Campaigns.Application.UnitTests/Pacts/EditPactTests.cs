using FluentAssertions;
using Lanka.Common.Application.Authentication;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Application.Pacts.Edit;
using Lanka.Modules.Campaigns.Application.Pacts.GetBloggerPact;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using Lanka.Modules.Campaigns.Domain.Pacts;
using NSubstitute;

namespace Lanka.Modules.Campaigns.Application.UnitTests.Pacts;

#pragma warning disable CA1515 // Type can be made internal
public class EditPactTests
#pragma warning disable
{
    private static EditPactCommand Command => new(PactData.Content);
    
    private readonly IPactRepository _pactRepositoryMock;
    private readonly IUserContext _userContextMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    
    private readonly EditPactCommandHandler _handler;
    
    public EditPactTests()
    {
        this._pactRepositoryMock = Substitute.For<IPactRepository>();
        this._userContextMock = Substitute.For<IUserContext>();
        this._unitOfWorkMock = Substitute.For<IUnitOfWork>();
        
        this._handler = new EditPactCommandHandler(
            this._pactRepositoryMock,
            this._userContextMock,
            this._unitOfWorkMock
        );
    }
    
    [Fact]
    public async Task Handle_ShouldReturnPactResponse()
    {
        // Arrange
        this._userContextMock.GetUserId()
            .Returns(Guid.NewGuid());

        this._pactRepositoryMock.GetByBloggerIdAsync(
            Arg.Any<BloggerId>(),
            Arg.Any<CancellationToken>()
        ).Returns(PactData.CreatePact());
        
        
        // Act
        Result<PactResponse> result = await this._handler.Handle(
            Command,
            CancellationToken.None
        );
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }
    
    [Fact]
    public async Task Handle_ShouldReturnNotFoundError_WhenPactNotFound()
    {
        // Arrange
        this._userContextMock.GetUserId()
            .Returns(Guid.NewGuid());

        this._pactRepositoryMock.GetByBloggerIdAsync(
            Arg.Any<BloggerId>(),
            Arg.Any<CancellationToken>()
        ).Returns((Pact?)null);
        
        
        // Act
        Result<PactResponse> result = await this._handler.Handle(
            Command,
            CancellationToken.None
        );
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PactErrors.NotFound);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenPactUpdateFails()
    {
        // Arrange
        this._userContextMock.GetUserId()
            .Returns(Guid.NewGuid());

        this._pactRepositoryMock.GetByBloggerIdAsync(
            Arg.Any<BloggerId>(),
            Arg.Any<CancellationToken>()
        ).Returns(PactData.CreatePact());
        
        var command = new EditPactCommand(string.Empty);
        
        // Act
        Result<PactResponse> result = await this._handler.Handle(
            command,
            CancellationToken.None
        );
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }
}
