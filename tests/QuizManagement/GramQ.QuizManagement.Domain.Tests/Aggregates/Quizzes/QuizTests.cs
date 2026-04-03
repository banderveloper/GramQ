using FluentAssertions;

using GramQ.QuizManagement.Domain.Aggregates.Quizzes;
using GramQ.QuizManagement.Domain.Aggregates.Quizzes.Errors;

namespace GramQ.QuizManagement.Domain.Tests.Aggregates.Quizzes;

public class QuizTests
{
    [Fact]
    public void Create_ValidParameters_ReturnsSuccess()
    {
        // Arrange
        var validId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        const string validTitle = "Valid Quiz Title";
        var validCreatedBy = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var validNow = DateTimeOffset.Parse("2024-01-01T00:00:00Z");

        // Act
        var result = Quiz.Create(validId, validTitle, validCreatedBy, validNow);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(validId);
        result.Value.Title.Should().Be(validTitle);
        result.Value.Status.Should().Be(QuizStatus.Draft);
    }

    [Fact]
    public void Create_EmptyId_ThrowsArgumentException()
    {
        // Act
        var act = () => Quiz.Create(Guid.Empty, "some title", Guid.NewGuid(), DateTimeOffset.UtcNow);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_EmptyCreatedBy_ThrowsArgumentException()
    {
        // Act
        var act = () => Quiz.Create(Guid.NewGuid(), "some title", Guid.Empty, DateTimeOffset.UtcNow);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_EmptyTitle_ThrowsArgumentException()
    {
        // Act
        var act = () => Quiz.Create(Guid.NewGuid(), string.Empty, Guid.NewGuid(), DateTimeOffset.UtcNow);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_TitleTooLong_ReturnsFailure()
    {
        // Arrange
        const uint longTitleLength = QuizRules.MaxQuizTitleLength + 1;

        // Act
        var result = Quiz.Create(Guid.NewGuid(), new string('a', (int)longTitleLength), Guid.NewGuid(),
            DateTimeOffset.UtcNow);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(QuizErrors.Quiz.TitleTooLong(
            longTitleLength,
            QuizRules.MaxQuizTitleLength));
    }
}
