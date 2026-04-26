namespace GramQ.Api.Requests.AnswerOption;

public sealed record UpdateAnswerOptionRequest
{
    public required string Text { get; init; }
    public bool IsCorrect { get; init; }
};
