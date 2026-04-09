using GramQ.QuizManagement.Application.Abstractions;
using GramQ.QuizManagement.Domain.Aggregates.Quizzes;
using GramQ.QuizManagement.Domain.Aggregates.Quizzes.Errors;
using GramQ.Shared.Abstractions.Models;

namespace GramQ.QuizManagement.Application.UseCases.Queries;

public sealed record GetQuizByIdQuery(Guid QuizId);

public sealed record GetQuizByIdQueryResult(
    Guid Id,
    string Title,
    QuizStatus Status,
    Guid CreatedBy,
    DateTimeOffset CreatedAt,
    Guid? UpdatedBy,
    DateTimeOffset? UpdatedAt,
    IReadOnlyList<GetQuizByIdQueryResult.Question> Questions
)
{
    public sealed record Question(
        Guid Id,
        string Text,
        uint Order,
        uint TimeLimitSeconds,
        uint Points,
        IReadOnlyList<AnswerOption> AnswerOptions);

    public sealed record AnswerOption(
        Guid Id,
        string Text,
        uint Order,
        bool IsCorrect);
};

public sealed class GetQuizByIdQueryHandler(
    IQuizRepository quizRepository,
    ICurrentUserContext currentUser)
{
    public async Task<Result<GetQuizByIdQueryResult>> HandleAsync(
        GetQuizByIdQuery query,
        CancellationToken cancellationToken)
    {
        var quiz = await quizRepository.GetByIdAsync(query.QuizId, cancellationToken);

        if (quiz is null)
            return QuizErrors.Quiz.NotFound(query.QuizId);

        if (quiz.CreatedBy != currentUser.UserId && !currentUser.IsAdmin)
            return QuizErrors.Quiz.Forbidden;

        return MapToUseCaseResult(quiz);
    }

    private static GetQuizByIdQueryResult MapToUseCaseResult(Quiz quiz)
    {
        return new GetQuizByIdQueryResult(
            Id: quiz.Id,
            Title: quiz.Title,
            Status: quiz.Status,
            CreatedBy: quiz.CreatedBy,
            CreatedAt: quiz.CreatedAt,
            UpdatedAt: quiz.UpdatedAt,
            UpdatedBy: quiz.UpdatedBy,
            Questions: quiz.Questions.Select(MapQuestion).OrderBy(q => q.Order).ToList()
        );

        static GetQuizByIdQueryResult.Question MapQuestion(Question question)
            => new GetQuizByIdQueryResult.Question(
                Id: question.Id,
                Text: question.Text,
                Order: question.Order,
                TimeLimitSeconds: question.TimeLimitSeconds,
                Points: question.Points,
                AnswerOptions: question.AnswerOptions.Select(MapAnswerOption).OrderBy(ao => ao.Order).ToList());

        static GetQuizByIdQueryResult.AnswerOption MapAnswerOption(AnswerOption answerOption)
            => new GetQuizByIdQueryResult.AnswerOption(
                Id: answerOption.Id,
                Text: answerOption.Text,
                Order: answerOption.Order,
                IsCorrect: answerOption.IsCorrect);
    }
}
