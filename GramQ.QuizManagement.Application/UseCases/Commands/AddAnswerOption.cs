using GramQ.QuizManagement.Application.Abstractions;
using GramQ.QuizManagement.Domain.Aggregates.Quizzes.Errors;
using GramQ.Shared.Abstractions.Models;
using GramQ.Shared.Abstractions.Time;

namespace GramQ.QuizManagement.Application.UseCases.Commands;

public sealed record AddAnswerOptionCommand(Guid QuizId, Guid QuestionId, string Text, bool IsCorrect);

public sealed class AddAnswerOptionCommandHandler(
    IQuizRepository quizRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserContext currentUser)
{
    public async Task<Result<Guid>> HandleAsync(AddAnswerOptionCommand command, CancellationToken cancellationToken)
    {
        var quiz = await quizRepository.GetByIdAsync(command.QuizId, cancellationToken);

        if (quiz is null)
            return QuizErrors.Quiz.NotFound(command.QuizId);

        if (quiz.CreatedBy != currentUser.UserId && !currentUser.IsAdmin)
            return QuizErrors.Quiz.Forbidden;

        var addAnswerOptionResult = quiz.AddAnswerOption(command.QuestionId, Guid.NewGuid(), command.Text,
            command.IsCorrect, currentUser.UserId, dateTimeProvider.UtcNow);

        if (addAnswerOptionResult.IsFailure)
            return addAnswerOptionResult.Error;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return addAnswerOptionResult.Value.Id;
    }
}
