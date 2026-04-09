using GramQ.QuizManagement.Application.Abstractions;
using GramQ.QuizManagement.Domain.Aggregates.Quizzes.Errors;
using GramQ.Shared.Abstractions.Models;
using GramQ.Shared.Abstractions.Time;

namespace GramQ.QuizManagement.Application.UseCases.Commands;

public sealed record AddQuestionCommand(Guid QuizId, string Text, uint TimeLimitSeconds, uint Points);

public sealed class AddQuestionCommandHandler(
    IQuizRepository quizRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserContext currentUser)
{
    public async Task<Result<Guid>> HandleAsync(AddQuestionCommand command, CancellationToken cancellationToken)
    {
        var quiz = await quizRepository.GetByIdAsync(command.QuizId, cancellationToken);

        if (quiz is null)
            return QuizErrors.Quiz.NotFound(command.QuizId);

        if (quiz.CreatedBy != currentUser.UserId && !currentUser.IsAdmin)
            return QuizErrors.Quiz.Forbidden;

        var addQuestionResult = quiz.AddQuestion(Guid.NewGuid(), command.Text, command.TimeLimitSeconds, command.Points,
            currentUser.UserId, dateTimeProvider.UtcNow);

        if (addQuestionResult.IsFailure)
            return addQuestionResult.Error;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return addQuestionResult.Value.Id;
    }
}
