using GramQ.QuizManagement.Application.Abstractions;
using GramQ.QuizManagement.Domain.Aggregates.Quizzes.Errors;
using GramQ.Shared.Abstractions.Models;
using GramQ.Shared.Abstractions.Time;

namespace GramQ.QuizManagement.Application.UseCases.Commands;

public sealed record UpdateAnswerOptionCommand(
    Guid QuizId,
    Guid QuestionId,
    Guid AnswerOptionId,
    string Text,
    bool IsCorrect);

public sealed class UpdateAnswerOptionCommandHandler(
    IQuizRepository quizRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserContext currentUser)
{
    public async Task<Result> HandleAsync(UpdateAnswerOptionCommand command, CancellationToken cancellationToken)
    {
        var quiz = await quizRepository.GetByIdAsync(command.QuizId, cancellationToken);

        if (quiz is null)
            return QuizErrors.Quiz.NotFound(command.QuizId);

        if (quiz.CreatedBy != currentUser.UserId && !currentUser.IsAdmin)
            return QuizErrors.Quiz.Forbidden;

        var updateAnswerOptionsResult = quiz.UpdateAnswerOption(command.QuestionId, command.AnswerOptionId,
            command.Text, command.IsCorrect, currentUser.UserId, dateTimeProvider.UtcNow);

        if (updateAnswerOptionsResult.IsFailure)
            return updateAnswerOptionsResult.Error;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
