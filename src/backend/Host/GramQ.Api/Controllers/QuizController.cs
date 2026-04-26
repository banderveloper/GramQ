using GramQ.Api.Controllers.Mappers;
using GramQ.Api.Requests;
using GramQ.QuizManagement.Application.Queries;
using GramQ.QuizManagement.Application.UseCases.Commands;
using GramQ.QuizManagement.Application.UseCases.Queries;

using Microsoft.AspNetCore.Mvc;

namespace GramQ.Api.Controllers;

[Route("api/quizzes")]
public sealed class QuizController : BaseController
{
    [HttpGet("{quizId:guid}")]
    public async Task<IActionResult> GetQuiz(
        [FromRoute] Guid quizId,
        [FromServices] GetQuizByIdQueryHandler handler)
    {
        var getQuizResult = await handler.HandleAsync(
            new GetQuizByIdQuery(quizId),
            HttpContext.RequestAborted);

        if (getQuizResult.IsFailure)
            return Problem(getQuizResult.Error);

        return Ok(getQuizResult.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetQuizzes(
        [FromQuery] GetQuizzesRequest request,
        [FromServices] GetQuizzesQueryHandler handler)
    {
        var getQuizzesResult = await handler.HandleAsync(
            QuizRequestMapper.ToQuery(request),
            HttpContext.RequestAborted);

        if (getQuizzesResult.IsFailure)
            return Problem(getQuizzesResult.Error);

        return Ok(getQuizzesResult.Value);
    }

    [HttpPost]
    public async Task<IActionResult> CreateQuiz(
        [FromBody] CreateQuizRequest request,
        [FromServices] CreateQuizCommandHandler handler)
    {
        var createQuizResult = await handler.HandleAsync(
            QuizRequestMapper.ToCommand(request),
            HttpContext.RequestAborted);

        if (createQuizResult.IsFailure)
            return Problem(createQuizResult.Error);

        return CreatedAtAction(
            nameof(GetQuiz),
            new { quizId = createQuizResult.Value },
            createQuizResult.Value);
    }

    [HttpPut("{quizId:guid}")]
    public async Task<IActionResult> UpdateQuiz(
        [FromRoute] Guid quizId,
        [FromBody] UpdateQuizRequest request,
        [FromServices] UpdateQuizCommandHandler handler)
    {
        var updateQuizResult = await handler.HandleAsync(
            new UpdateQuizCommand(quizId, request.Title),
            HttpContext.RequestAborted);

        if (updateQuizResult.IsFailure)
            return Problem(updateQuizResult.Error);

        return NoContent();
    }

    [HttpDelete("{quizId:guid}")]
    public async Task<IActionResult> DeleteQuiz(
        [FromRoute] Guid quizId,
        [FromServices] DeleteQuizCommandHandler handler)
    {
        var deleteQuizResult = await handler.HandleAsync(
            new DeleteQuizCommand(quizId),
            HttpContext.RequestAborted);

        if (deleteQuizResult.IsFailure)
            return Problem(deleteQuizResult.Error);

        return NoContent();
    }

    [HttpPut("{quizId:guid}/publish")]
    public async Task<IActionResult> PublishQuiz(
        [FromRoute] Guid quizId,
        [FromServices] PublishQuizCommandHandler handler)
    {
        var publishQuizResult = await handler.HandleAsync(
            new PublishQuizCommand(quizId),
            HttpContext.RequestAborted);

        if (publishQuizResult.IsFailure)
            return Problem(publishQuizResult.Error);

        return NoContent();
    }

    [HttpPut("{quizId:guid}/unpublish")]
    public async Task<IActionResult> UnpublishQuiz(
        [FromRoute] Guid quizId,
        [FromServices] UnpublishQuizCommandHandler handler)
    {
        var unpublishQuizResult = await handler.HandleAsync(
            new UnpublishQuizCommand(quizId),
            HttpContext.RequestAborted);

        if (unpublishQuizResult.IsFailure)
            return Problem(unpublishQuizResult.Error);

        return NoContent();
    }
}
