namespace GramQ.Api.Requests;

public sealed record CreateQuizRequest
{
    public required string Title { get; init; }
    public IReadOnlyList<Question> Questions { get; init; } = [];

    public sealed record Question
    {
        public required string Text { get; init; }
        public uint TimeLimitSeconds { get; init; }
        public uint Points { get; init; }
        public IReadOnlyList<AnswerOption> Answers { get; init; } = [];

        public sealed record AnswerOption
        {
            public required string Text { get; init; }
            public bool IsCorrect { get; init; }
        }
    }
};
