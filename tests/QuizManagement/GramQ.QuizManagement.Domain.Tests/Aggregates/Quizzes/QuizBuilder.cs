using GramQ.QuizManagement.Domain.Aggregates.Quizzes;
using GramQ.Shared.Abstractions.Models;

namespace GramQ.QuizManagement.Domain.Tests.Aggregates.Quizzes;

public class QuizBuilder
{
    public static readonly Guid DefaultQuizId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public static readonly Guid DefaultCreatedBy = Guid.Parse("00000000-0000-0000-0000-000000000002");
    public static readonly DateTimeOffset DefaultNow = DateTimeOffset.Parse("2024-01-01T00:00:00Z");
    public static readonly string DefaultQuizTitle = "SUT Quiz";

    private Guid _id = DefaultQuizId;
    private string _title = DefaultQuizTitle;
    private Guid _createdBy = DefaultCreatedBy;
    private DateTimeOffset _now = DefaultNow;

    public QuizBuilder WithId(Guid id) { _id = id; return this; }
    public QuizBuilder WithTitle(string title) { _title = title; return this; }
    public QuizBuilder WithCreatedBy(Guid createdBy) { _createdBy = createdBy; return this; }
    public QuizBuilder WithNow(DateTimeOffset now) { _now = now; return this; }

    public Quiz BuildDraft()
    {
        var quizCreateResult = Quiz.Create(_id, _title, _createdBy, _now);
        ThrowIfResultFailed(quizCreateResult);

        return quizCreateResult.Value;
    }

    public Quiz BuildPublished()
    {
        var questionId = Guid.Parse("00000000-0000-0000-0000-000000000003");
        var questionTitle = "SUT question";
        var questionTimeLimitSeconds = QuizRules.MaxTimeLimitSeconds - 1;
        uint questionPoints = 5;

        var firstAnswerOptionId = Guid.Parse("00000000-0000-0000-0000-000000000004");
        var firstAnswerText = "Incorrect answer option 1";
        var firstAnswerCorrect = false;

        var secondAnswerOptionId = Guid.Parse("00000000-0000-0000-0000-000000000005");
        var secondAnswerText = "Correct answer option 2";
        var secondAnswerCorrect = true;

        var quizCreateResult = Quiz.Create(DefaultQuizId, DefaultQuizTitle, DefaultCreatedBy, DefaultNow);
        ThrowIfResultFailed(quizCreateResult);

        var quiz = quizCreateResult.Value;

        var addQuestionResult = quiz.AddQuestion(questionId, questionTitle, questionTimeLimitSeconds, questionPoints, DefaultCreatedBy,
            DefaultNow);
        ThrowIfResultFailed(addQuestionResult);

        var addAnswerResult = quiz.AddAnswerOption(questionId, firstAnswerOptionId, firstAnswerText, firstAnswerCorrect, DefaultCreatedBy,
            DefaultNow);
        ThrowIfResultFailed(addAnswerResult);

        addAnswerResult = quiz.AddAnswerOption(questionId, secondAnswerOptionId, secondAnswerText, secondAnswerCorrect, DefaultCreatedBy,
            DefaultNow);
        ThrowIfResultFailed(addAnswerResult);

        var publishResult = quiz.Publish(DefaultCreatedBy, DefaultNow);
        ThrowIfResultFailed(publishResult);

        return quiz;
    }

    private void ThrowIfResultFailed(Result result)
    {
        if(result.IsFailure)
            throw new InvalidOperationException($"QuizBuilder failed: {result.Error}");
    }
    private void ThrowIfResultFailed<T>(Result<T> result) where T : notnull
    {
        if(result.IsFailure)
            throw new InvalidOperationException($"QuizBuilder failed: {result.Error}");
    }
}
