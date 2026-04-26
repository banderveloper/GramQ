using GramQ.Api.Requests.AnswerOption;
using GramQ.QuizManagement.Application.UseCases.Commands;

using Microsoft.AspNetCore.Mvc;

namespace GramQ.Api.Controllers;

[Route("api/quizzes/{quizId:guid}/questions/{questionId:guid}/answer-options")]
public class AnswerOptionController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> AddAnswerOption(
        [FromRoute] Guid quizId,
        [FromRoute] Guid questionId,
        [FromBody] AddAnswerOptionRequest request,
        [FromServices] AddAnswerOptionCommandHandler handler)
    {
        var addAnswerOptionResult = await handler.HandleAsync(
            new AddAnswerOptionCommand(quizId, questionId, request.Text, request.IsCorrect),
            HttpContext.RequestAborted);

        if (addAnswerOptionResult.IsFailure)
            return Problem(addAnswerOptionResult.Error);

        return Created(string.Empty, addAnswerOptionResult.Value);
    }

    [HttpPut("{answerOptionId:guid}")]
    public async Task<IActionResult> UpdateAnswerOption(
        [FromRoute] Guid quizId,
        [FromRoute] Guid questionId,
        [FromRoute] Guid answerOptionId,
        [FromBody] UpdateAnswerOptionRequest request,
        [FromServices] UpdateAnswerOptionCommandHandler handler)
    {
        var updateAnswerOptionResult = await handler.HandleAsync(
            new UpdateAnswerOptionCommand(quizId, questionId, answerOptionId, request.Text, request.IsCorrect),
            HttpContext.RequestAborted);

        if (updateAnswerOptionResult.IsFailure)
            return Problem(updateAnswerOptionResult.Error);

        return NoContent();
    }

    [HttpDelete("{answerOptionId:guid}")]
    public async Task<IActionResult> RemoveAnswerOption(
        [FromRoute] Guid quizId,
        [FromRoute] Guid questionId,
        [FromRoute] Guid answerOptionId,
        [FromServices] RemoveAnswerOptionCommandHandler handler)
    {
        var removeAnswerOptionResult = await handler.HandleAsync(
            new RemoveAnswerOptionCommand(quizId, questionId, answerOptionId),
            HttpContext.RequestAborted);

        if (removeAnswerOptionResult.IsFailure)
            return Problem(removeAnswerOptionResult.Error);

        return NoContent();
    }

    [HttpPut("reorder")]
    public async Task<IActionResult> ReorderAnswerOptions(
        [FromRoute] Guid quizId,
        [FromRoute] Guid questionId,
        [FromBody] ReorderAnswerOptionsRequest request,
        [FromServices] ReorderAnswersOptionCommandHandler handler)
    {
        var reorderAnswerOptionsResult = await handler.HandleAsync(
            new ReorderAnswersOptionCommand(quizId, questionId, request.AnswerOptionsIds),
            HttpContext.RequestAborted);

        if (reorderAnswerOptionsResult.IsFailure)
            return Problem(reorderAnswerOptionsResult.Error);

        return NoContent();
    }
}
