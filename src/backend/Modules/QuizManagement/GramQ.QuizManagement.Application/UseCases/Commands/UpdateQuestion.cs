using GramQ.QuizManagement.Application.Abstractions;
using GramQ.QuizManagement.Domain.Aggregates.Quizzes.Errors;
using GramQ.Shared.Abstractions.Models;
using GramQ.Shared.Abstractions.Time;

namespace GramQ.QuizManagement.Application.UseCases.Commands;

public sealed record UpdateQuestionCommand(
    Guid QuizId,
    Guid QuestionId,
    string Text,
    uint TimeLimitSeconds,
    uint Points);

public sealed class UpdateQuestionCommandHandler(
    IQuizRepository quizRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserContext currentUser)
{
    public async Task<Result> HandleAsync(UpdateQuestionCommand command, CancellationToken cancellationToken)
    {
        var quiz = await quizRepository.GetByIdAsync(command.QuizId, cancellationToken);

        if (quiz is null)
            return QuizErrors.Quiz.NotFound(command.QuizId);

        if (quiz.CreatedBy != currentUser.UserId && !currentUser.IsAdmin)
            return QuizErrors.Quiz.Forbidden;

        var updateQuestionResult = quiz.UpdateQuestion(command.QuestionId, command.Text, command.TimeLimitSeconds,
            command.Points, currentUser.UserId, dateTimeProvider.UtcNow);

        if (updateQuestionResult.IsFailure)
            return updateQuestionResult.Error;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
