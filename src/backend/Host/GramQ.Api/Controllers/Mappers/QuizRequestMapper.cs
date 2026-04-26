using GramQ.Api.Requests;
using GramQ.QuizManagement.Application.Queries;
using GramQ.QuizManagement.Application.UseCases.Commands;
using GramQ.QuizManagement.Application.UseCases.Queries;

namespace GramQ.Api.Controllers.Mappers;

public static class QuizRequestMapper
{
    // API GetQuizzesRequest => Handler GetQuizzesQuery
    public static GetQuizzesQuery ToQuery(GetQuizzesRequest request)
        => new(new QuizFilter
        {
            Page = request.Page,
            PageSize = request.PageSize,
            SortBy = request.SortBy ?? QuizFilter.QuizSortBy.CreatedAt,
            SortDescending = request.SortDescending,
            Status = request.Status,
            TitleContains = request.TitleContains
        });

    // API CreateQuizRequest => Handler CreateQuizCommand
    public static CreateQuizCommand ToCommand(CreateQuizRequest request) =>
        new(
            Title: request.Title,
            Questions: request.Questions
                .Select(q => new CreateQuizCommand.Question(
                    Text: q.Text,
                    TimeLimitSeconds: q.TimeLimitSeconds,
                    Points: q.Points,
                    AnswerOptions: q.Answers
                        .Select(ao => new CreateQuizCommand.AnswerOption(
                            Text: ao.Text,
                            IsCorrect: ao.IsCorrect))
                        .ToArray()))
                .ToArray());
}
