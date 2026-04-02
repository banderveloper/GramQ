using GramQ.QuizManagement.Domain.Aggregates.Quizzes.Errors;
using GramQ.Shared.Abstractions.Domain;
using GramQ.Shared.Abstractions.Models;

namespace GramQ.QuizManagement.Domain.Aggregates.Quizzes;

public class Question : Entity
{
    public string Text { get; private set; }
    public uint Order { get; private set; }
    public uint TimeLimitSeconds { get; private set; }
    public uint Points { get; private set; }

    private readonly List<AnswerOption> _answerOptions = [];
    public IReadOnlyCollection<AnswerOption> AnswerOptions => _answerOptions.AsReadOnly();

    public static Result<Question> Create(Guid id, string text, uint order, uint timeLimitSeconds, uint points)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Question id can't be default");

        if (text is null)
            throw new ArgumentNullException(nameof(text), "Question text can't be null");

        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Question text can't be empty", nameof(text));

        if (text.Length > QuizRules.MaxQuestionTextLength)
            return Result<Question>.Failure(QuizErrors.Question.TextTooLong((uint)text.Length,
                QuizRules.MaxQuestionTextLength));

        if (order is < 1 or > QuizRules.MaxQuestionsPerQuiz)
            return Result<Question>.Failure(QuizErrors.Question.OrderOutOfBounds(order, QuizRules.MaxQuestionsPerQuiz));

        if (timeLimitSeconds is < QuizRules.MinTimeLimitSeconds or > QuizRules.MaxTimeLimitSeconds)
            return Result<Question>.Failure(QuizErrors.Question.TimeLimitOutOfBounds(timeLimitSeconds,
                QuizRules.MinTimeLimitSeconds, QuizRules.MaxTimeLimitSeconds));

        return new Question(id, text, order, timeLimitSeconds, points);
    }

    public Result<AnswerOption> AddAnswerOption(Guid id, string text, bool isCorrect)
    {
        if (_answerOptions.Count >= QuizRules.MaxAnswerOptionsPerQuestion)
            return Result<AnswerOption>.Failure(
                QuizErrors.Question.AnswerOptionsLimitReached(
                    (uint)_answerOptions.Count + 1,
                    QuizRules.MaxAnswerOptionsPerQuestion));

        if (isCorrect && _answerOptions.Any(ao => ao.IsCorrect))
            return Result<AnswerOption>.Failure(QuizErrors.Question.AlreadyHasCorrectAnswer);

        var order = (uint)_answerOptions.Count + 1;

        var createAnswerOptionResult = AnswerOption.Create(id, text, isCorrect, order);

        if (createAnswerOptionResult.IsFailure)
            return createAnswerOptionResult.Error;

        _answerOptions.Add(createAnswerOptionResult.Value);

        return createAnswerOptionResult.Value;
    }

    public Result RemoveAnswerOption(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Answer option id can't be default");

        var option = _answerOptions.FirstOrDefault(ao => ao.Id == id);
        if (option is null)
            return Result.Failure(QuizErrors.Question.AnswerOptionsNotFound(id));

        if (option.IsCorrect)
            return Result.Failure(QuizErrors.Question.LastCorrectAnswerDelete);

        _answerOptions.RemoveAll(ao => ao.Id == id);

        return Result.Success();
    }

    public Result Update(string text, uint timeLimitSeconds, uint points)
    {
        if (text is null)
            throw new ArgumentNullException(nameof(text), "Question text can't be null");

        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Question text can't be empty", nameof(text));

        if (text.Length > QuizRules.MaxQuestionTextLength)
            return Result.Failure(QuizErrors.Question.TextTooLong((uint)text.Length,
                QuizRules.MaxQuestionTextLength));

        if (timeLimitSeconds is < QuizRules.MinTimeLimitSeconds or > QuizRules.MaxTimeLimitSeconds)
            return Result.Failure(QuizErrors.Question.TimeLimitOutOfBounds(timeLimitSeconds,
                QuizRules.MinTimeLimitSeconds, QuizRules.MaxTimeLimitSeconds));

        Text = text;
        TimeLimitSeconds = timeLimitSeconds;
        Points = points;

        return Result.Success();
    }

    public Result UpdateAnswerOption(Guid id, string text, bool isCorrect)
    {
        var option = _answerOptions.FirstOrDefault(ao => ao.Id == id);
        if (option is null)
            return Result.Failure(QuizErrors.Question.AnswerOptionsNotFound(id));

        if (isCorrect && option.IsCorrect == false && _answerOptions.Any(ao => ao.IsCorrect))
            return Result.Failure(QuizErrors.Question.AlreadyHasCorrectAnswer);

        return option.Update(text, isCorrect);
    }

    private Question(Guid id, string text, uint order, uint timeLimitSeconds, uint points) : base(id)
    {
        Text = text;
        Order = order;
        TimeLimitSeconds = timeLimitSeconds;
        Points = points;
    }

    private Question()
    {
        Text = null!;
    }
}
