namespace GramQ.Api.Requests.Question;

public record ReorderQuestionsRequest
{
    public IReadOnlyList<Guid> QuestionsIds { get; init; } = [];
};
