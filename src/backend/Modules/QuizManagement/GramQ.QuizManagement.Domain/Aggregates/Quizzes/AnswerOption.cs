using GramQ.QuizManagement.Domain.Aggregates.Quizzes.Errors;
using GramQ.Shared.Abstractions.Domain;
using GramQ.Shared.Abstractions.Models;

namespace GramQ.QuizManagement.Domain.Aggregates.Quizzes;

public class AnswerOption : Entity
{
    public string Text { get; private set; }
    public bool IsCorrect { get; private set; }
    public uint Order { get; private set; }

    internal static Result<AnswerOption> Create(Guid id, string text, bool isCorrect, uint order)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        if (text.Length > QuizRules.MaxAnswerOptionTextLength)
            return Result<AnswerOption>.Failure(
                QuizErrors.AnswerOption.TextTooLong((uint)text.Length, QuizRules.MaxAnswerOptionTextLength));

        if (order is > QuizRules.MaxAnswerOptionsPerQuestion or < 1)
            return Result<AnswerOption>.Failure(
                QuizErrors.AnswerOption.OrderOutOfBounds(order, QuizRules.MaxAnswerOptionsPerQuestion));

        return new AnswerOption(id, text, isCorrect, order);
    }

    internal Result Update(string text, bool isCorrect)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        if (text.Length > QuizRules.MaxAnswerOptionTextLength)
            return Result.Failure(
                QuizErrors.AnswerOption.TextTooLong((uint)text.Length, QuizRules.MaxAnswerOptionTextLength));

        Text = text;
        IsCorrect = isCorrect;
        return Result.Success();
    }

    internal void SetOrder(uint order)
    {
        Order = order;
    }

    private AnswerOption(Guid id, string text, bool isCorrect, uint order) : base(id)
    {
        Text = text;
        IsCorrect = isCorrect;
        Order = order;
    }

    /// <summary>Required by EF Core. Do not use in application code.</summary>
    private AnswerOption()
    {
        Text = null!;
    }
}
