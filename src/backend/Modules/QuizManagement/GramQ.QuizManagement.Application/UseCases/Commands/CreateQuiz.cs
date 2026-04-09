using GramQ.QuizManagement.Application.Abstractions;
using GramQ.QuizManagement.Domain.Aggregates.Quizzes;
using GramQ.Shared.Abstractions.Models;
using GramQ.Shared.Abstractions.Time;

namespace GramQ.QuizManagement.Application.UseCases.Commands;

public sealed record CreateQuizCommand(
    string Title,
    CreateQuizCommand.Question[] Questions)
{
    public sealed record Question(
        string Text,
        uint TimeLimitSeconds,
        uint Points,
        AnswerOption[] AnswerOptions);

    public sealed record AnswerOption(
        string Text,
        bool IsCorrect);
}

public sealed class CreateQuizCommandHandler(
    IQuizRepository quizRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserContext currentUser)
{
    public async Task<Result<Guid>> HandleAsync(CreateQuizCommand command, CancellationToken cancellationToken)
    {
        var quizCreateResult = Quiz.Create(Guid.NewGuid(), command.Title, currentUser.UserId, dateTimeProvider.UtcNow);

        if(quizCreateResult.IsFailure)
            return quizCreateResult.Error;

        var quiz = quizCreateResult.Value;

        foreach (CreateQuizCommand.Question question in command.Questions)
        {
            var questionId = Guid.NewGuid();
            var addQuestionResult = quiz.AddQuestion(questionId, question.Text, question.TimeLimitSeconds,
                question.Points, currentUser.UserId, dateTimeProvider.UtcNow);

            if (addQuestionResult.IsFailure)
                return addQuestionResult.Error;

            foreach (CreateQuizCommand.AnswerOption answerOption in question.AnswerOptions)
            {
                var answerOptionId = Guid.NewGuid();
                var addAnswerOptionResult = quiz.AddAnswerOption(questionId, answerOptionId, answerOption.Text,
                    answerOption.IsCorrect, currentUser.UserId, dateTimeProvider.UtcNow);

                if (addAnswerOptionResult.IsFailure)
                    return addAnswerOptionResult.Error;
            }
        }

        quizRepository.Add(quiz);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return quiz.Id;
    }
}
