namespace GramQ.Api.Requests.Question;

public sealed record AddQuestionRequest
{
    public required string Text { get; init; }
    public uint TimeLimitSeconds { get; init; }
    public uint Points { get; init; }
}
