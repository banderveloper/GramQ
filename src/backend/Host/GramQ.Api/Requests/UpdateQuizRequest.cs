namespace GramQ.Api.Requests;

public sealed record UpdateQuizRequest
{
    public required string Title { get; init; }
};
