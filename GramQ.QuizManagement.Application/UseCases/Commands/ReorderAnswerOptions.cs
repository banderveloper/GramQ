using GramQ.QuizManagement.Application.Abstractions;
using GramQ.QuizManagement.Domain.Aggregates.Quizzes.Errors;
using GramQ.Shared.Abstractions.Models;
using GramQ.Shared.Abstractions.Time;

namespace GramQ.QuizManagement.Application.UseCases.Commands;

public sealed record ReorderAnswersOptionCommand(
    Guid QuizId,
    Guid QuestionId,
    IReadOnlyList<Guid> AnswerOptionsIds);

public sealed class ReorderAnswersOptionCommandHandler(
    IQuizRepository quizRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserContext currentUser)
{
    public async Task<Result> HandleAsync(ReorderAnswersOptionCommand command, CancellationToken cancellationToken)
    {
        var quiz = await quizRepository.GetByIdAsync(command.QuizId, cancellationToken);

        if (quiz is null)
            return QuizErrors.Quiz.NotFound(command.QuizId);

        if (quiz.CreatedBy != currentUser.UserId && !currentUser.IsAdmin)
            return QuizErrors.Quiz.Forbidden;

        var reorderAnswerOptionsResult = quiz.ReorderAnswerOptions(command.QuestionId, command.AnswerOptionsIds,
            currentUser.UserId, dateTimeProvider.UtcNow);

        if (reorderAnswerOptionsResult.IsFailure)
            return reorderAnswerOptionsResult.Error;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
