using FluentAssertions;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Domain.Reviews;
using Lanka.Modules.Campaigns.UnitTests.Abstractions;

namespace Lanka.Modules.Campaigns.UnitTests.Reviews;

public class ReviewTests : BaseTest
{
    [Fact]
    public void Create_ShouldReturnReview_WhenCampaignIsCompleted()
    {
        // Act
        Result<Review> result = Review.Create(
            ReviewData.CompletedCooperation,
            ReviewData.ValidRating,
            ReviewData.ValidComment,
            ReviewData.CreatedOnUtc
        );

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenRatingIsInvalid()
    {
        // Act
        Result<Review> result = Review.Create(
            ReviewData.CompletedCooperation,
            0,
            ReviewData.ValidComment,
            ReviewData.CreatedOnUtc
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenCommentIsInvalid()
    {
        // Act
        Result<Review> result = Review.Create(
            ReviewData.CompletedCooperation,
            ReviewData.ValidRating,
            ReviewData.InvalidComment,
            ReviewData.CreatedOnUtc
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenCampaignIsNotCompleted()
    {
        // Act
        Result<Review> result = Review.Create(
            ReviewData.NotCompletedCooperation,
            ReviewData.ValidRating,
            ReviewData.ValidComment,
            ReviewData.CreatedOnUtc
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Update_ShouldReturnSuccess_WhenReviewIsUpdated()
    {
        // Arrange
        Review review = Review.Create(
            ReviewData.CompletedCooperation,
            ReviewData.ValidRating,
            ReviewData.ValidComment,
            ReviewData.CreatedOnUtc
        ).Value;

        // Act
        Result result = review.Update(
            4,
            "Updated comment"
        );

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        review.Comment.Value.Should().Be("Updated comment");
    }

    [Fact]
    public void Update_ShouldReturnFailure_WhenRatingIsInvalid()
    {
        // Arrange
        Review review = Review.Create(
            ReviewData.CompletedCooperation,
            ReviewData.ValidRating,
            ReviewData.ValidComment,
            ReviewData.CreatedOnUtc
        ).Value;

        // Act
        Result result = review.Update(
            0,
            "Updated comment"
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void Update_ShouldReturnFailure_WhenCommentIsInvalid()
    {
        // Arrange
        Review review = Review.Create(
            ReviewData.CompletedCooperation,
            ReviewData.ValidRating,
            ReviewData.ValidComment,
            ReviewData.CreatedOnUtc
        ).Value;

        // Act
        Result result = review.Update(
            4,
            string.Empty
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }
}
