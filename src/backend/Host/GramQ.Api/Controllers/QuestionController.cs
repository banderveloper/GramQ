using GramQ.Api.Requests.Question;
using GramQ.QuizManagement.Application.UseCases.Commands;

using Microsoft.AspNetCore.Mvc;

namespace GramQ.Api.Controllers;

[Route("api/quizzes/{quizId:guid}/questions")]
public class QuestionController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> AddQuestion(
        [FromRoute] Guid quizId,
        [FromBody] AddQuestionRequest request,
        [FromServices] AddQuestionCommandHandler handler)
    {
        var addQuestionResult = await handler.HandleAsync(
            new AddQuestionCommand(quizId, request.Text, request.TimeLimitSeconds, request.Points),
            HttpContext.RequestAborted);

        if (addQuestionResult.IsFailure)
            return Problem(addQuestionResult.Error);

        return Created(string.Empty, addQuestionResult.Value);
    }

    [HttpPut("{questionId:guid}")]
    public async Task<IActionResult> UpdateQuestion(
        [FromRoute] Guid quizId,
        [FromRoute] Guid questionId,
        [FromBody] UpdateQuestionRequest request,
        [FromServices] UpdateQuestionCommandHandler handler)
    {
        var updateQuestionResult = await handler.HandleAsync(
            new UpdateQuestionCommand(quizId, questionId, request.Text, request.TimeLimitSeconds, request.Points),
            HttpContext.RequestAborted);

        if (updateQuestionResult.IsFailure)
            return Problem(updateQuestionResult.Error);

        return NoContent();
    }

    [HttpDelete("{questionId:guid}")]
    public async Task<IActionResult> RemoveQuestion(
        [FromRoute] Guid quizId,
        [FromRoute] Guid questionId,
        [FromServices] RemoveQuestionCommandHandler handler)
    {
        var removeQuestionResult = await handler.HandleAsync(
            new RemoveQuestionCommand(quizId, questionId),
            HttpContext.RequestAborted);

        if (removeQuestionResult.IsFailure)
            return Problem(removeQuestionResult.Error);

        return NoContent();
    }

    [HttpPut("reorder")]
    public async Task<IActionResult> ReorderQuestions(
        [FromRoute] Guid quizId,
        [FromBody] ReorderQuestionsRequest request,
        [FromServices] ReorderQuestionsCommandHandler handler)
    {
        var reorderQuestionsResult = await handler.HandleAsync(
            new ReorderQuestionsCommand(quizId, request.QuestionsIds),
            HttpContext.RequestAborted);

        if (reorderQuestionsResult.IsFailure)
            return Problem(reorderQuestionsResult.Error);

        return NoContent();
    }
}
