using GramQ.QuizManagement.Application.Abstractions;
using GramQ.QuizManagement.Domain.Aggregates.Quizzes.Errors;
using GramQ.Shared.Abstractions.Models;
using GramQ.Shared.Abstractions.Time;

namespace GramQ.QuizManagement.Application.UseCases.Commands;

public sealed record ReorderAnswerOptionCommand(
    Guid QuizId,
    Guid QuestionId,
    IReadOnlyList<Guid> AnswerOptionsIds,
    Guid ReorderedBy);

public sealed class ReorderAnswerOptionCommandHandler(
    IQuizRepository quizRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
{
    public async Task<Result> HandleAsync(ReorderAnswerOptionCommand command, CancellationToken cancellationToken)
    {
        var quiz = await quizRepository.GetByIdAsync(command.QuizId, cancellationToken);

        if (quiz is null)
            return QuizErrors.Quiz.NotFound(command.QuizId);

        var reorderAnswerOptionsResult = quiz.ReorderAnswerOptions(command.QuestionId, command.AnswerOptionsIds,
            command.ReorderedBy, dateTimeProvider.UtcNow);
        if (reorderAnswerOptionsResult.IsFailure)
            return reorderAnswerOptionsResult.Error;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
