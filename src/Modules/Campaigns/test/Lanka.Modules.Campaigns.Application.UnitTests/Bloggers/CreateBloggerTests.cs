using FluentAssertions;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Data;
using Lanka.Modules.Campaigns.Application.Bloggers.Create;
using Lanka.Modules.Campaigns.Domain.Bloggers;
using NSubstitute;

namespace Lanka.Modules.Campaigns.Application.UnitTests.Bloggers;

#pragma warning disable CA1515
public class CreateBloggerTests
{
    private static CreateBloggerCommand Command => new(
        Guid.NewGuid(),
        BloggerData.Email,
        BloggerData.FirstName,
        BloggerData.Email,
        BloggerData.BirthDate
    );

    private readonly IBloggerRepository _bloggerRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;

    private readonly CreateBloggerCommandHandler _handler;

    public CreateBloggerTests()
    {
        this._bloggerRepositoryMock = Substitute.For<IBloggerRepository>();
        this._unitOfWorkMock = Substitute.For<IUnitOfWork>();

        this._handler = new CreateBloggerCommandHandler(
            this._bloggerRepositoryMock,
            this._unitOfWorkMock
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess()
    {
        // Act
        Result result = await this._handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await this._unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
