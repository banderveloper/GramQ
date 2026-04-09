using GramQ.QuizManagement.Application.Abstractions;
using GramQ.QuizManagement.Domain.Aggregates.Quizzes.Errors;
using GramQ.Shared.Abstractions.Models;
using GramQ.Shared.Abstractions.Time;

namespace GramQ.QuizManagement.Application.UseCases.Commands;

public sealed record RemoveAnswerOptionCommand(Guid QuizId, Guid QuestionId, Guid AnswerOptionId);

public sealed class RemoveAnswerOptionCommandHandler(
    IQuizRepository quizRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserContext currentUser)
{
    public async Task<Result> HandleAsync(RemoveAnswerOptionCommand command, CancellationToken cancellationToken)
    {
        var quiz = await quizRepository.GetByIdAsync(command.QuizId, cancellationToken);

        if (quiz is null)
            return QuizErrors.Quiz.NotFound(command.QuizId);

        if (quiz.CreatedBy != currentUser.UserId && !currentUser.IsAdmin)
            return QuizErrors.Quiz.Forbidden;

        var removeAnswerOptionResult = quiz.RemoveAnswerOption(command.QuestionId, command.AnswerOptionId,
            currentUser.UserId, dateTimeProvider.UtcNow);

        if (removeAnswerOptionResult.IsFailure)
            return removeAnswerOptionResult.Error;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
