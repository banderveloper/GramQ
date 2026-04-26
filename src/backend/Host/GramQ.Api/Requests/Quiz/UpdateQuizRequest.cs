namespace GramQ.Api.Requests.Quiz;

public sealed record UpdateQuizRequest
{
    public required string Title { get; init; }
};
