using GramQ.QuizManagement.Domain.Aggregates.Quizzes.Errors;
using GramQ.QuizManagement.Domain.Events;
using GramQ.Shared.Abstractions.Domain;
using GramQ.Shared.Abstractions.Models;
using GramQ.Shared.Guards;

namespace GramQ.QuizManagement.Domain.Aggregates.Quizzes;

public class Quiz : AggregateRoot, IAuditable, ISoftDeletable
{
    public string Title { get; private set; }
    public QuizStatus Status { get; private set; }

    private readonly List<Question> _questions = [];
    public IReadOnlyCollection<Question> Questions => _questions.AsReadOnly();

    public DateTimeOffset CreatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public static Result<Quiz> Create(Guid id, string title, Guid createdBy, DateTimeOffset now)
    {
        Guard.ThrowIfDefault(id, nameof(id));
        Guard.ThrowIfDefault(createdBy, nameof(createdBy));
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        // if title is too long
        if(title.Length > QuizRules.MaxQuizTitleLength)
            return Result<Quiz>.Failure(QuizErrors.Quiz.TitleTooLong((uint)title.Length, QuizRules.MaxQuizTitleLength));

        return new Quiz(id, title, createdBy, now);
    }

    public Result Update(string title, Guid updatedBy, DateTimeOffset now)
    {
        Guard.ThrowIfDefault(updatedBy, nameof(updatedBy));
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        // mutation in draft mode is restricted
        var draftCheck = EnsureDraft();
        if (draftCheck.IsFailure) return draftCheck;

        // if title too long
        if(title.Length > QuizRules.MaxQuizTitleLength)
            return Result.Failure(QuizErrors.Quiz.TitleTooLong((uint)title.Length, QuizRules.MaxQuizTitleLength));

        Title = title;

        Touch(updatedBy, now);

        return Result.Success();
    }

    public Result Publish(Guid updatedBy, DateTimeOffset now)
    {
        Guard.ThrowIfDefault(updatedBy, nameof(updatedBy));

        if(Status == QuizStatus.Published)
            return Result.Failure(QuizErrors.Quiz.AlreadyPublished);

        if(_questions.Count < 1)
            return Result.Failure(QuizErrors.Quiz.NoQuestions);

        // at least one questions has too few answer options
        if (_questions.Any(q => q.AnswerOptions.Count < QuizRules.MinAnswerOptionsPerQuestion))
            return Result.Failure(QuizErrors.Quiz.QuestionsWithLackingAnswersExists);

        // at least one question hasn't correct answer
        if (_questions.Any(q => q.AnswerOptions.All(option => !option.IsCorrect)))
            return Result.Failure(QuizErrors.Quiz.QuestionsWithoutCorrectAnswerExists);

        Status = QuizStatus.Published;
        Touch(updatedBy, now);

        RaiseDomainEvent(new QuizPublishedEvent(Guid.NewGuid(), now, Id, updatedBy));

        return Result.Success();
    }

    public Result Unpublish(Guid updatedBy, DateTimeOffset now)
    {
        Guard.ThrowIfDefault(updatedBy, nameof(updatedBy));

        if(Status == QuizStatus.Draft)
            return Result.Failure(QuizErrors.Quiz.AlreadyInDraft);

        Status = QuizStatus.Draft;

        Touch(updatedBy, now);

        RaiseDomainEvent(new QuizUnpublishedEvent(Guid.NewGuid(), now, Id, updatedBy));

        return Result.Success();
    }

    public Result Delete(Guid deletedBy, DateTimeOffset now)
    {
        Guard.ThrowIfDefault(deletedBy, nameof(deletedBy));

        if (IsDeleted)
            return Result.Failure(QuizErrors.Quiz.AlreadyDeleted);

        IsDeleted = true;
        DeletedAt = now;

        Touch(deletedBy, now);

        RaiseDomainEvent(new QuizDeletedEvent(Guid.NewGuid(), now, Id, deletedBy));

        return Result.Success();
    }

    public Result<Question> AddQuestion(Guid id, string text, uint timeLimitSeconds, uint points,
        Guid updatedBy, DateTimeOffset now)
    {
        Guard.ThrowIfDefault(id, nameof(id));
        Guard.ThrowIfDefault(updatedBy, nameof(updatedBy));

        // mutation in draft mode is restricted
        var draftCheck = EnsureDraft();
        if (draftCheck.IsFailure) return draftCheck;

        // if max questions limit reached
        if (_questions.Count >= QuizRules.MaxQuestionsPerQuiz)
            return Result<Question>.Failure(QuizErrors.Quiz.QuestionsCountLimitReached(QuizRules.MaxQuestionsPerQuiz));

        var order = (uint)_questions.Count + 1;
        var createQuestionResult = Question.Create(id, text, order, timeLimitSeconds, points);

        if (createQuestionResult.IsFailure)
            return createQuestionResult;

        _questions.Add(createQuestionResult.Value);

        Touch(updatedBy, now);

        return createQuestionResult;
    }

    public Result RemoveQuestion(Guid id, Guid updatedBy, DateTimeOffset now)
    {
        Guard.ThrowIfDefault(id, nameof(id));
        Guard.ThrowIfDefault(updatedBy, nameof(updatedBy));

        // mutation in draft mode is restricted
        var draftCheck = EnsureDraft();
        if (draftCheck.IsFailure) return draftCheck;

        var questionToDelete = _questions.FirstOrDefault(q => q.Id == id);

        if (questionToDelete is null)
            return Result.Failure(QuizErrors.Quiz.QuestionNotFound(id));

        _questions.RemoveAll(q => q.Id == id);

        for (var i = 0; i < _questions.Count; i++)
            _questions[i].SetOrder((uint)(i + 1));

        Touch(updatedBy, now);

        return Result.Success();
    }

    public Result UpdateQuestion(Guid id, string text, uint timeLimitSeconds, uint points,
        Guid updatedBy, DateTimeOffset now)
    {
        Guard.ThrowIfDefault(id, nameof(id));
        Guard.ThrowIfDefault(updatedBy, nameof(updatedBy));

        // mutation in draft mode is restricted
        var draftCheck = EnsureDraft();
        if (draftCheck.IsFailure) return draftCheck;

        var questionToUpdate = _questions.FirstOrDefault(q => q.Id == id);

        if (questionToUpdate is null)
            return Result.Failure(QuizErrors.Quiz.QuestionNotFound(id));

        var updateQuestionResult = questionToUpdate.Update(text, timeLimitSeconds, points);

        if (updateQuestionResult.IsFailure)
            return updateQuestionResult;

        Touch(updatedBy, now);

        return updateQuestionResult;
    }

    public Result ReorderQuestions(IReadOnlyList<Guid> orderedQuestionsIds, Guid updatedBy, DateTimeOffset now)
    {
        Guard.ThrowIfDefault(updatedBy, nameof(updatedBy));

        // mutation in draft mode is restricted
        var draftCheck = EnsureDraft();
        if (draftCheck.IsFailure) return draftCheck;

        // count mismatch
        if (orderedQuestionsIds.Count != _questions.Count)
            return Result.Failure(
                QuizErrors.Quiz.ReorderCountMismatch(
                    (uint)_questions.Count, (uint)orderedQuestionsIds.Count));

        var existingIds = _questions.Select(ao => ao.Id).ToHashSet();
        var seen = new HashSet<Guid>(orderedQuestionsIds.Count);

        foreach (var id in orderedQuestionsIds)
        {
            if (!seen.Add(id))
                return Result.Failure(QuizErrors.Quiz.ReorderDuplicatingElements);

            // reordered questions contain odd elements or doesn't contains required elements
            if (!existingIds.Contains(id))
                return Result.Failure(QuizErrors.Quiz.ReorderMismatch);
        }

        for (var i = 0; i < orderedQuestionsIds.Count; i++)
            _questions.First(ao => ao.Id == orderedQuestionsIds[i]).SetOrder((uint)(i + 1));

        Touch(updatedBy, now);

        return Result.Success();
    }

    public Result<AnswerOption> AddAnswerOption(Guid questionId, Guid answerOptionId, string text, bool isCorrect,
        Guid updatedBy, DateTimeOffset now)
    {
        Guard.ThrowIfDefault(questionId, nameof(questionId));
        Guard.ThrowIfDefault(answerOptionId, nameof(answerOptionId));

        // mutation in draft mode is restricted
        var draftCheck = EnsureDraft();
        if (draftCheck.IsFailure) return draftCheck;

        var questionToAdd = _questions.FirstOrDefault(q => q.Id == questionId);

        if (questionToAdd is null)
            return Result<AnswerOption>.Failure(QuizErrors.Quiz.QuestionNotFound(questionId));

        var addAnswerOptionResult = questionToAdd.AddAnswerOption(answerOptionId, text, isCorrect);

        if (addAnswerOptionResult.IsFailure)
            return addAnswerOptionResult;

        Touch(updatedBy, now);

        return addAnswerOptionResult;
    }

    public Result UpdateAnswerOption(Guid questionId, Guid answerOptionId, string text, bool isCorrect,
        Guid updatedBy, DateTimeOffset now)
    {
        Guard.ThrowIfDefault(questionId, nameof(questionId));
        Guard.ThrowIfDefault(answerOptionId, nameof(answerOptionId));

        // mutation in draft mode is restricted
        var draftCheck = EnsureDraft();
        if (draftCheck.IsFailure) return draftCheck;

        var questionToUpdate = _questions.FirstOrDefault(q => q.Id == questionId);

        if (questionToUpdate is null)
            return Result.Failure(QuizErrors.Quiz.QuestionNotFound(questionId));

        var updateAnswerOptionResult = questionToUpdate.UpdateAnswerOption(answerOptionId, text, isCorrect);

        if (updateAnswerOptionResult.IsFailure)
            return updateAnswerOptionResult;

        Touch(updatedBy, now);

        return updateAnswerOptionResult;
    }

    public Result RemoveAnswerOption(Guid questionId, Guid answerOptionId, Guid updatedBy, DateTimeOffset now)
    {
        Guard.ThrowIfDefault(questionId, nameof(questionId));
        Guard.ThrowIfDefault(answerOptionId, nameof(answerOptionId));

        // mutation in draft mode is restricted
        var draftCheck = EnsureDraft();
        if (draftCheck.IsFailure) return draftCheck;

        var questionToUpdate = _questions.FirstOrDefault(q => q.Id == questionId);

        if (questionToUpdate is null)
            return Result.Failure(QuizErrors.Quiz.QuestionNotFound(questionId));

        var removeAnswerOptionResult = questionToUpdate.RemoveAnswerOption(answerOptionId);

        if (removeAnswerOptionResult.IsFailure)
            return removeAnswerOptionResult;

        Touch(updatedBy, now);

        return removeAnswerOptionResult;
    }

    public Result ReorderAnswerOptions(Guid questionId, IReadOnlyList<Guid> orderedAnswersIds,
        Guid updatedBy, DateTimeOffset now)
    {
        Guard.ThrowIfDefault(questionId, nameof(questionId));
        Guard.ThrowIfDefault(updatedBy, nameof(updatedBy));

        // mutation in draft mode is restricted
        var draftCheck = EnsureDraft();
        if (draftCheck.IsFailure) return draftCheck;

        var questionToUpdate = _questions.FirstOrDefault(q => q.Id == questionId);

        if (questionToUpdate is null)
            return Result.Failure(QuizErrors.Quiz.QuestionNotFound(questionId));

        var reorderAnswerOptionsResult = questionToUpdate.ReorderAnswerOptions(orderedAnswersIds);

        if (reorderAnswerOptionsResult.IsFailure)
            return reorderAnswerOptionsResult;

        Touch(updatedBy, now);

        return reorderAnswerOptionsResult;
    }

    private Quiz(Guid id, string title, Guid creatorId, DateTimeOffset now)
    {
        Id = id;
        Title = title;
        CreatedBy = creatorId;
        CreatedAt = now;
        UpdatedAt = null;
        UpdatedBy = null;
        IsDeleted = false;
        DeletedAt = null;
        Status = QuizStatus.Draft;
    }

    /// <summary>Required by EF Core. Do not use in application code.</summary>
    private Quiz()
    {
        Title = null!;
    }

    private void Touch(Guid updatedBy, DateTimeOffset now)
    {
        UpdatedBy = updatedBy;
        UpdatedAt = now;
    }

    private Result EnsureDraft()
    {
        if (Status != QuizStatus.Draft)
            return Result.Failure(QuizErrors.Quiz.MutationNotInDraft);
        return Result.Success();
    }
}
