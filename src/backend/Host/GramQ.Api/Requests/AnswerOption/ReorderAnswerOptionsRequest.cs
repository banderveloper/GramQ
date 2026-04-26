namespace GramQ.Api.Requests.AnswerOption;

public sealed record ReorderAnswerOptionsRequest
{
    public IReadOnlyList<Guid> AnswerOptionsIds { get; init; } = [];
};
