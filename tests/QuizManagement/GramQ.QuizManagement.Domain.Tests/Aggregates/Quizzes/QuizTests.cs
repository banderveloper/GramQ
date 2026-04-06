using FluentAssertions;

using GramQ.QuizManagement.Domain.Aggregates.Quizzes;
using GramQ.QuizManagement.Domain.Aggregates.Quizzes.Errors;
using GramQ.QuizManagement.Domain.Events;

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

    [Fact]
    public void Publish_ValidQuiz_ReturnsSuccess()
    {
        // Arrange
        var firstQuestionId = Guid.Parse("29320022-BD72-4294-9D93-AE20AE7E5250");
        var secondQuestionId = Guid.Parse("36DDACD1-FD75-4BCD-BF2C-34F3BF202087");
        var quiz = new QuizBuilder().BuildDraft();

        quiz.AddQuestion(firstQuestionId, "Question 1",
            QuizRules.MaxTimeLimitSeconds - 1, 15, quiz.CreatedBy, quiz.CreatedAt);
        quiz.AddAnswerOption(firstQuestionId, Guid.Parse("D6398524-1A36-4A4E-AD08-4336E7C0CABA"), "AO 1", true,
            quiz.CreatedBy, quiz.CreatedAt);
        quiz.AddAnswerOption(firstQuestionId, Guid.Parse("6743DF0D-718B-4BDF-ADEF-EF069DB0330E"), "AO 2", false,
            quiz.CreatedBy, quiz.CreatedAt);

        quiz.AddQuestion(secondQuestionId, "Question 2",
            QuizRules.MaxTimeLimitSeconds - 1, 15, quiz.CreatedBy, quiz.CreatedAt);
        quiz.AddAnswerOption(secondQuestionId, Guid.Parse("9615000C-F57C-4DE2-9E3E-EFC724B77F84"), "AO 1", true,
            quiz.CreatedBy, quiz.CreatedAt);
        quiz.AddAnswerOption(secondQuestionId, Guid.Parse("6FBB3F59-8D88-402C-A8A5-0D19448134C3"), "AO 2", false,
            quiz.CreatedBy, quiz.CreatedAt);

        // Act
        var publishResult = quiz.Publish(quiz.CreatedBy, quiz.CreatedAt);

        // Assert
        publishResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Publish_ValidQuiz_RaisesQuizPublishedEvent()
    {
        // Arrange Act
        var quiz = new QuizBuilder().BuildPublished();

        // Assert
        quiz.DomainEvents.OfType<QuizPublishedEvent>().Should().ContainSingle();
        quiz.DomainEvents.First().As<QuizPublishedEvent>().QuizId.Should().Be(quiz.Id);
        quiz.DomainEvents.First().As<QuizPublishedEvent>().PublishedBy.Should().Be(quiz.CreatedBy);
    }

    [Fact]
    public void Publish_AlreadyPublished_ReturnsAlreadyPublishedError()
    {
        // Arrange
        var quiz = new QuizBuilder().BuildPublished();

        // Act
        var publishResult = quiz.Publish(quiz.CreatedBy, quiz.CreatedAt);

        // Assert
        publishResult.IsFailure.Should().BeTrue();
        publishResult.Error.Should().Be(QuizErrors.Quiz.AlreadyPublished);
    }

    [Fact]
    public void Publish_NoQuestions_ReturnsNoQuestionsError()
    {
        // Arrange
        var quiz = new QuizBuilder().BuildDraft();

        // Act
        var publishResult = quiz.Publish(quiz.CreatedBy, quiz.CreatedAt);

        // Assert
        publishResult.IsFailure.Should().BeTrue();
        publishResult.Error.Should().Be(QuizErrors.Quiz.NoQuestions);
    }

    [Fact]
    public void Publish_WithQuestionWithLackingAnswers_ReturnsQuestionsWithLackingAnswersError()
    {
        // Arrange
        var firstQuestionId = Guid.Parse("29320022-BD72-4294-9D93-AE20AE7E5250");
        var quiz = new QuizBuilder().BuildDraft();

        quiz.AddQuestion(firstQuestionId, "Question 1",
            QuizRules.MaxTimeLimitSeconds - 1, 15, quiz.CreatedBy, quiz.CreatedAt);
        quiz.AddAnswerOption(firstQuestionId, Guid.Parse("D6398524-1A36-4A4E-AD08-4336E7C0CABA"), "AO 1", true,
            quiz.CreatedBy, quiz.CreatedAt);

        // Act
        var publishResult = quiz.Publish(quiz.CreatedBy, quiz.CreatedAt);

        // Assert
        publishResult.IsFailure.Should().BeTrue();
        publishResult.Error.Should().Be(QuizErrors.Quiz.QuestionsWithLackingAnswersExists);
    }

    [Fact]
    public void Publish_QuestionWithoutCorrectAnswer_ReturnsQuestionsWithoutCorrectAnswerExistsError()
    {
        // Arrange
        var firstQuestionId = Guid.Parse("29320022-BD72-4294-9D93-AE20AE7E5250");
        var quiz = new QuizBuilder().BuildDraft();

        quiz.AddQuestion(firstQuestionId, "Question 1",
            QuizRules.MaxTimeLimitSeconds - 1, 15, quiz.CreatedBy, quiz.CreatedAt);
        quiz.AddAnswerOption(firstQuestionId, Guid.Parse("D6398524-1A36-4A4E-AD08-4336E7C0CABA"), "AO 1", false,
            quiz.CreatedBy, quiz.CreatedAt);
        quiz.AddAnswerOption(firstQuestionId, Guid.Parse("6743DF0D-718B-4BDF-ADEF-EF069DB0330E"), "AO 2", false,
            quiz.CreatedBy, quiz.CreatedAt);

        // Act
        var publishResult = quiz.Publish(quiz.CreatedBy, quiz.CreatedAt);

        // Assert
        publishResult.IsFailure.Should().BeTrue();
        publishResult.Error.Should().Be(QuizErrors.Quiz.QuestionsWithoutCorrectAnswerExists);
    }

    [Fact]
    public void Unpublish_PublishedQuiz_ReturnsSuccess()
    {
        // Arrange
        var quiz = new QuizBuilder().BuildPublished();

        // Act
        var unpublishResult = quiz.Unpublish(quiz.CreatedBy, quiz.CreatedAt);

        // Assert
        unpublishResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Unpublish_PublishedQuiz_RaisesQuizUnpublishedEvent()
    {
        // Arrange
        var quiz = new QuizBuilder().BuildPublished();

        // Act
        quiz.Unpublish(quiz.CreatedBy, quiz.CreatedAt);
        var unpublishedEvent = quiz.DomainEvents
            .OfType<QuizUnpublishedEvent>()
            .Single();

        // Assert
        unpublishedEvent.QuizId.Should().Be(quiz.Id);
        unpublishedEvent.UnpublishedBy.Should().Be(quiz.CreatedBy);
    }

    [Fact]
    public void Unpublish_DraftQuiz_ReturnsAlreadyInDraftError()
    {
        // Arrange
        var quiz = new QuizBuilder().BuildDraft();

        // Act
        var unpublishResult = quiz.Unpublish(quiz.CreatedBy, quiz.CreatedAt);

        // Assert
        unpublishResult.IsFailure.Should().BeTrue();
        unpublishResult.Error.Should().Be(QuizErrors.Quiz.AlreadyInDraft);
    }

    [Fact]
    public void Unpublish_EmptyUpdatedBy_ThrowsArgumentException()
    {
        // Arrange
        var quiz = new QuizBuilder().BuildPublished();

        // Act
        var unpublishFunc = () => quiz.Unpublish(Guid.Empty, quiz.CreatedAt);

        // Assert
        unpublishFunc.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Delete_ValidQuiz_ReturnsSuccess()
    {
        // Arrange
        var quiz = new QuizBuilder().BuildPublished();

        // Act
        var deleteResult = quiz.Delete(quiz.CreatedBy, quiz.CreatedAt);

        // Assert
        deleteResult.IsSuccess.Should().BeTrue();
        quiz.IsDeleted.Should().BeTrue();
        quiz.DeletedAt.Should().Be(quiz.CreatedAt);
    }

    [Fact]
    public void Delete_ValidQuiz_RaisesQuizDeletedEvent()
    {
        // Arrange
        var quiz = new QuizBuilder().BuildPublished();

        // Act
        quiz.Delete(quiz.CreatedBy, quiz.CreatedAt);

        // Assert
        var deletedEvent = quiz.DomainEvents
            .OfType<QuizDeletedEvent>()
            .Single();

        // Assert
        deletedEvent.QuizId.Should().Be(quiz.Id);
        deletedEvent.DeletedBy.Should().Be(quiz.CreatedBy);
    }

    [Fact]
    public void Delete_DeletedQuiz_ReturnsAlreadyDeletedError()
    {
        // Arrange
        var quiz = new QuizBuilder().BuildPublished();

        // Act
        quiz.Delete(quiz.CreatedBy, quiz.CreatedAt);
        var secondDeleteResult = quiz.Delete(quiz.CreatedBy, quiz.CreatedAt);

        // Assert
        secondDeleteResult.IsFailure.Should().BeTrue();
        secondDeleteResult.Error.Should().Be(QuizErrors.Quiz.AlreadyDeleted);
    }

    [Fact]
    public void Delete_EmptyDeletedBy_ThrowsArgumentException()
    {
        // Arrange
        var quiz = new QuizBuilder().BuildPublished();

        // Act
        var deleteFunc = () => quiz.Delete(Guid.Empty, quiz.CreatedAt);

        // Assert
        deleteFunc.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddQuestion_ValidQuestion_ReturnsSuccess()
    {
        var questionId = Guid.Parse("29320022-BD72-4294-9D93-AE20AE7E5250");
        var quiz = new QuizBuilder().BuildDraft();

        var addQuestionResult = quiz.AddQuestion(questionId, "Question 1",
            QuizRules.MinTimeLimitSeconds + 1, 15, quiz.CreatedBy, quiz.CreatedAt);

        addQuestionResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void AddQuestion_QuizPublished_ReturnsMutationNotInDraftError()
    {
        var questionId = Guid.Parse("29320022-BD72-4294-9D93-AE20AE7E5250");
        var quiz = new QuizBuilder().BuildPublished();

        var addQuestionResult = quiz.AddQuestion(questionId, "Question 1",
            QuizRules.MinTimeLimitSeconds + 1, 15, quiz.CreatedBy, quiz.CreatedAt);

        addQuestionResult.IsFailure.Should().BeTrue();
        addQuestionResult.Error.Should().Be(QuizErrors.Quiz.MutationNotInDraft);
    }

    [Fact]
    public void AddQuestion_QuestionsOutOfLimit_ReturnsQuestionsCountLimitReachedError()
    {
        // Arrange
        var quiz = new QuizBuilder().BuildDraft();
        for (var i = 0; i < QuizRules.MaxQuestionsPerQuiz; i++)
            quiz.AddQuestion(Guid.NewGuid(), $"Question {i}", 15, 15, quiz.CreatedBy, quiz.CreatedAt);

        // Act
        var addOddQuestionResult = quiz.AddQuestion(Guid.Parse("CBC53FDB-CC4A-41E7-AA09-20A04F0DEF39"), "Odd question",
            QuizRules.MinTimeLimitSeconds + 1, 15, quiz.CreatedBy, quiz.CreatedAt);

        // Assert
        addOddQuestionResult.IsFailure.Should().BeTrue();
        addOddQuestionResult.Error.Should()
            .Be(QuizErrors.Quiz.QuestionsCountLimitReached(QuizRules.MaxQuestionsPerQuiz));
    }

    [Fact]
    public void AddQuestion_EmptyQuizId_ThrowsArgumentException()
    {
        // Arrange
        var quiz = new QuizBuilder().BuildDraft();

        // Act
        var addQuestionFunc = () => quiz.AddQuestion(Guid.Empty, "Question 1",
            QuizRules.MinTimeLimitSeconds + 1, 15, quiz.CreatedBy, quiz.CreatedAt);

        // Assert
        addQuestionFunc.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddQuestion_EmptyCreatorId_ThrowsArgumentException()
    {
        // Arrange
        var quiz = new QuizBuilder().BuildDraft();

        // Act
        var addQuestionFunc = () => quiz.AddQuestion(Guid.Parse("832315C3-7C56-4BB9-8D27-306511EDAF72"), "Question 1",
            15, 15, Guid.Empty, quiz.CreatedAt);

        // Assert
        addQuestionFunc.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddQuestion_InvalidTimeLimit_ReturnsFailure()
    {
        // Arrange
        var quiz = new QuizBuilder().BuildDraft();

        // Act
        var addQuestionResult = quiz.AddQuestion(Guid.Parse("CBC53FDB-CC4A-41E7-AA09-20A04F0DEF39"), "Odd question",
            QuizRules.MinTimeLimitSeconds - 1, 15, quiz.CreatedBy, quiz.CreatedAt);

        // Assert
        addQuestionResult.IsFailure.Should().BeTrue();
        addQuestionResult.Error.Should().Be(QuizErrors.Question.TimeLimitOutOfBounds(QuizRules.MinTimeLimitSeconds - 1,
            QuizRules.MinTimeLimitSeconds, QuizRules.MaxTimeLimitSeconds));
    }

    [Fact]
    public void RemoveQuestion_ValidState_ReturnsSuccess()
    {
        // Arrange
        var questionId = Guid.Parse("E1A68C02-848A-4D5E-8E1E-8EC3386C59AA");
        var quiz = new QuizBuilder().BuildDraft();
        quiz.AddQuestion(questionId, "Question 1", QuizRules.MinTimeLimitSeconds + 1, 15, quiz.CreatedBy, quiz.CreatedAt);

        // Act
        var removeQuestionResult = quiz.RemoveQuestion(questionId, quiz.CreatedBy, quiz.CreatedAt);

        // Assert
        removeQuestionResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void RemoveQuestion_QuizPublished_ReturnsMutationNotInDraftError()
    {
        // Arrange
        var quiz = new QuizBuilder().BuildPublished();

        // Act
        var removeQuestionResult = quiz.RemoveQuestion(quiz.Questions.First().Id, quiz.CreatedBy, quiz.CreatedAt);

        // Assert
        removeQuestionResult.IsFailure.Should().BeTrue();
        removeQuestionResult.Error.Should().Be(QuizErrors.Quiz.MutationNotInDraft);
    }

    [Fact]
    public void RemoveQuestion_QuestionNotFound_ReturnsQuestionNotFoundError()
    {
        // Arrange
        var questionId = Guid.Parse("E1A68C02-848A-4D5E-8E1E-8EC3386C59AA");
        var nonExistingIdQuestionId = Guid.Parse("944D93C5-601D-4D18-866C-E69967DE2DB9");

        var quiz = new QuizBuilder().BuildDraft();
        quiz.AddQuestion(questionId, "Question 1", QuizRules.MinTimeLimitSeconds + 1, 15, quiz.CreatedBy, quiz.CreatedAt);

        // Act
        var removeQuestionResult = quiz.RemoveQuestion(nonExistingIdQuestionId, quiz.CreatedBy, quiz.CreatedAt);

        // Assert
        removeQuestionResult.IsFailure.Should().BeTrue();
        removeQuestionResult.Error.Should().Be(QuizErrors.Quiz.QuestionNotFound(nonExistingIdQuestionId));
    }

    [Fact]
    public void RemoveQuestion_EmptyQuestionId_ThrowsArgumentException()
    {
        // Arrange
        var questionId = Guid.Parse("E1A68C02-848A-4D5E-8E1E-8EC3386C59AA");
        var quiz = new QuizBuilder().BuildDraft();
        quiz.AddQuestion(questionId, "Question 1", QuizRules.MinTimeLimitSeconds + 1, 15, quiz.CreatedBy, quiz.CreatedAt);

        // Act
        var removeQuestionFunc = () => quiz.RemoveQuestion(Guid.Empty, quiz.CreatedBy, quiz.CreatedAt);

        // Assert
        removeQuestionFunc.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RemoveQuestion_EmptyUpdatedBy_ThrowsArgumentException()
    {
        // Arrange
        var questionId = Guid.Parse("E1A68C02-848A-4D5E-8E1E-8EC3386C59AA");
        var quiz = new QuizBuilder().BuildDraft();
        quiz.AddQuestion(questionId, "Question 1", QuizRules.MinTimeLimitSeconds + 1, 15, quiz.CreatedBy, quiz.CreatedAt);

        // Act
        var removeQuestionFunc = () => quiz.RemoveQuestion(questionId, Guid.Empty, quiz.CreatedAt);

        // Assert
        removeQuestionFunc.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RemoveQuestion_MiddleQuestion_ReordersRemainingQuestions()
    {
        // Arrange
        var firstQuestionId = Guid.Parse("E1A68C02-848A-4D5E-8E1E-8EC3386C59AA");
        var secondQuestionId = Guid.Parse("944D93C5-601D-4D18-866C-E69967DE2DB9");
        var thirdQuestionId = Guid.Parse("CBC53FDB-CC4A-41E7-AA09-20A04F0DEF39");

        var quiz = new QuizBuilder().BuildDraft();
        quiz.AddQuestion(firstQuestionId, "Question 1", QuizRules.MinTimeLimitSeconds + 1, 15, quiz.CreatedBy, quiz.CreatedAt);
        quiz.AddQuestion(secondQuestionId, "Question 2", QuizRules.MinTimeLimitSeconds + 1, 15, quiz.CreatedBy, quiz.CreatedAt);
        quiz.AddQuestion(thirdQuestionId, "Question 3", QuizRules.MinTimeLimitSeconds + 1, 15, quiz.CreatedBy, quiz.CreatedAt);

        // Act
        quiz.RemoveQuestion(secondQuestionId, quiz.CreatedBy, quiz.CreatedAt);

        // Assert
        quiz.Questions.Should().HaveCount(2);
        quiz.Questions.First().Order.Should().Be(1);
        quiz.Questions.Last().Order.Should().Be(2);
    }
}
