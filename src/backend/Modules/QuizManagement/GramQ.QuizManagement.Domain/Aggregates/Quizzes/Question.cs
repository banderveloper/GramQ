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

    internal static Result<Question> Create(Guid id, string text, uint order, uint timeLimitSeconds, uint points)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        // if text too long
        if (text.Length > QuizRules.MaxQuestionTextLength)
            return Result<Question>.Failure(QuizErrors.Question.TextTooLong((uint)text.Length,
                QuizRules.MaxQuestionTextLength));

        // if order out of range
        if (order is < 1 or > QuizRules.MaxQuestionsPerQuiz)
            return Result<Question>.Failure(QuizErrors.Question.OrderOutOfBounds(order, QuizRules.MaxQuestionsPerQuiz));

        // if time limit out of range
        if (timeLimitSeconds is < QuizRules.MinTimeLimitSeconds or > QuizRules.MaxTimeLimitSeconds)
            return Result<Question>.Failure(QuizErrors.Question.TimeLimitOutOfBounds(timeLimitSeconds,
                QuizRules.MinTimeLimitSeconds, QuizRules.MaxTimeLimitSeconds));

        return new Question(id, text, order, timeLimitSeconds, points);
    }

    internal Result<AnswerOption> AddAnswerOption(Guid id, string text, bool isCorrect)
    {
        // if answers count will be more than max
        if (_answerOptions.Count >= QuizRules.MaxAnswerOptionsPerQuestion)
            return Result<AnswerOption>.Failure(
                QuizErrors.Question.AnswerOptionsLimitReached(
                    (uint)_answerOptions.Count + 1,
                    QuizRules.MaxAnswerOptionsPerQuestion));

        // if any answer with this text
        if(_answerOptions.Any(option => string.Equals(option.Text, text, StringComparison.InvariantCultureIgnoreCase)))
            return Result<AnswerOption>.Failure(QuizErrors.Question.AlreadyHasAnswerWithText(text));

        // if correct answer already exists
        if (isCorrect && _answerOptions.Any(ao => ao.IsCorrect))
            return Result<AnswerOption>.Failure(QuizErrors.Question.AlreadyHasCorrectAnswer);

        var order = (uint)_answerOptions.Count + 1;

        var createAnswerOptionResult = AnswerOption.Create(id, text, isCorrect, order);

        if (createAnswerOptionResult.IsFailure)
            return createAnswerOptionResult.Error;

        _answerOptions.Add(createAnswerOptionResult.Value);

        return createAnswerOptionResult.Value;
    }

    internal Result RemoveAnswerOption(Guid id)
    {
        var option = _answerOptions.FirstOrDefault(ao => ao.Id == id);
        if (option is null)
            return Result.Failure(QuizErrors.Question.AnswerOptionNotFound(id));

        if (option.IsCorrect)
            return Result.Failure(QuizErrors.Question.LastCorrectAnswerDelete);

        _answerOptions.RemoveAll(ao => ao.Id == id);

        for (var i = 0; i < _answerOptions.Count; i++)
            _answerOptions[i].SetOrder((uint)(i + 1));

        return Result.Success();
    }

    internal Result Update(string text, uint timeLimitSeconds, uint points)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        // if text too long
        if (text.Length > QuizRules.MaxQuestionTextLength)
            return Result.Failure(QuizErrors.Question.TextTooLong((uint)text.Length,
                QuizRules.MaxQuestionTextLength));

        // if time limit out of range
        if (timeLimitSeconds is < QuizRules.MinTimeLimitSeconds or > QuizRules.MaxTimeLimitSeconds)
            return Result.Failure(QuizErrors.Question.TimeLimitOutOfBounds(timeLimitSeconds,
                QuizRules.MinTimeLimitSeconds, QuizRules.MaxTimeLimitSeconds));

        Text = text;
        TimeLimitSeconds = timeLimitSeconds;
        Points = points;

        return Result.Success();
    }

    internal Result UpdateAnswerOption(Guid id, string text, bool isCorrect)
    {
        var option = _answerOptions.FirstOrDefault(ao => ao.Id == id);
        if (option is null)
            return Result.Failure(QuizErrors.Question.AnswerOptionNotFound(id));

        // if question already has correct answer
        if (isCorrect && option.IsCorrect == false && _answerOptions.Any(ao => ao.IsCorrect))
            return Result.Failure(QuizErrors.Question.AlreadyHasCorrectAnswer);

        return option.Update(text, isCorrect);
    }

    internal void SetOrder(uint order)
    {
        Order = order;
    }

    internal Result ReorderAnswerOptions(IReadOnlyList<Guid> orderedAnswerOptionsIds)
    {
        // if count mismatch
        if (orderedAnswerOptionsIds.Count != _answerOptions.Count)
            return Result.Failure(
                QuizErrors.Question.ReorderCountMismatch(
                    (uint)_answerOptions.Count, (uint)orderedAnswerOptionsIds.Count));

        var existingIds = _answerOptions.Select(ao => ao.Id).ToHashSet();
        var seen = new HashSet<Guid>(orderedAnswerOptionsIds.Count);

        foreach (var id in orderedAnswerOptionsIds)
        {
            if (!seen.Add(id))
                return Result.Failure(QuizErrors.Question.ReorderDuplicatingElements);

            // reordered answers contain odd elements or doesn't contains required elements
            if (!existingIds.Contains(id))
                return Result.Failure(QuizErrors.Question.ReorderMismatch);
        }

        for (var i = 0; i < orderedAnswerOptionsIds.Count; i++)
            _answerOptions.First(ao => ao.Id == orderedAnswerOptionsIds[i]).SetOrder((uint)(i + 1));

        return Result.Success();
    }

    private Question(Guid id, string text, uint order, uint timeLimitSeconds, uint points) : base(id)
    {
        Text = text;
        Order = order;
        TimeLimitSeconds = timeLimitSeconds;
        Points = points;
    }

    /// <summary>Required by EF Core. Do not use in application code.</summary>
    private Question()
    {
        Text = null!;
    }
}
