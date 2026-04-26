namespace GramQ.Api.Requests.Question;

public sealed record ReorderQuestionsRequest
{
    public IReadOnlyList<Guid> QuestionsIds { get; init; } = [];
};
