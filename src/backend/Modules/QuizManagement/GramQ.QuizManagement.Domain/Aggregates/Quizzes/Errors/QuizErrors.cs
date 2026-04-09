using GramQ.Shared.Abstractions.Models;

namespace GramQ.QuizManagement.Domain.Aggregates.Quizzes.Errors;

public static class QuizErrors
{
    public static class AnswerOption
    {
        public static Error TextTooLong(uint current, uint max) =>
            Error.Validation(
                "AnswerOption.TextTooLong",
                $"Answer option text is too long: {current}/{max} characters.");

        public static Error OrderOutOfBounds(uint current, uint max) =>
            Error.Validation(
                "AnswerOption.OrderOutOfBounds",
                $"Answer option order out of bounds: current {current}, available 1-{max}");
    }

    public static class Question
    {
        public static Error TextTooLong(uint current, uint max) =>
            Error.Validation(
                "Question.TextTooLong",
                $"Question text is too long: {current}/{max} characters.");

        public static Error OrderOutOfBounds(uint current, uint max) =>
            Error.Validation(
                "Question.OrderOutOfBounds",
                $"Question order out of bounds: current {current}, available 1-{max}");

        public static Error TimeLimitOutOfBounds(uint current, uint min, uint max) =>
            Error.Validation(
                "Question.TimeLimitOutOfBounds",
                $"Question time limit out of bounds: current {current}, available {min}-{max}");

        public static Error AnswerOptionsLimitReached(uint currentOptions, uint maxOptions) =>
            Error.Validation(
                "Question.AnswerOptionsLimitReached",
                $"Question answer options limit reached: current {currentOptions}, max {maxOptions}");

        public static readonly Error AnswerOptionsEmpty =
            Error.Conflict(
                "Question.AnswerOptionsEmpty",
                $"Question answer options empty, nothing to remove");

        public static readonly Error AlreadyHasCorrectAnswer =
            Error.Conflict(
                "Question.AlreadyHasCorrectAnswer",
                $"Question already has correct answer");

        public static Error AlreadyHasAnswerWithText(string text) =>
            Error.Conflict(
                "Question.AlreadyHasAnswerWithText",
                $"Question already has answer with text '{text}'");

        public static readonly Error LastCorrectAnswerDelete =
            Error.Conflict(
                "Question.LastCorrectAnswerDelete",
                $"Can't delete last answer that is correct");

        public static Error AnswerOptionNotFound(Guid answerOptionId) =>
            Error.NotFound(
                "Question.AnswerOptionNotFound",
                $"Question answer option with given id not found: id {answerOptionId}");

        public static Error ReorderCountMismatch(uint expected, uint actual) =>
            Error.Conflict(
                "Question.ReorderCountMismatch",
                $"Question reordered answers count not equals to actual answers count: expected {expected}, actual {actual}");

        public static readonly Error ReorderMismatch =
            Error.Conflict(
                "Question.ReorderMismatch",
                $"Question reordered answers contains odd elements or doesn't contains required elements");

        public static readonly Error ReorderDuplicatingElements =
            Error.Conflict(
                "Question.ReorderDuplicatingElements",
                $"Question reordered answers contains duplicating elements");
    }

    public static class Quiz
    {
        public static readonly Error MutationNotInDraft =
            Error.Conflict(
                "Quiz.MutationNotInDraft",
                $"Quiz mutation not in draft mode is restricted");

        public static Error NotFound(Guid quizId) =>
            Error.NotFound(
                "Quiz.NotFound",
                $"Quiz with id '{quizId}' not found");

        public static readonly Error Forbidden =
            Error.Forbidden(
                "Quiz.Forbidden",
                $"Quiz operation rejected, neither owner nor admin");

        public static Error QuestionsCountLimitReached(uint max) =>
            Error.Validation(
                "Quiz.QuestionsCountLimitReached",
                $"Quiz questions count limit reached: max {max}");

        public static Error QuestionNotFound(Guid id) =>
            Error.NotFound(
                "Quiz.QuestionNotFound",
                $"Quiz question with given id not found: id {id}");

        public static Error TitleTooLong(uint current, uint max) =>
            Error.Validation(
                "Quiz.TitleTooLong",
                $"Quiz title length out of limit: current {current}, max {max}");

        public static readonly Error AlreadyDeleted =
            Error.Conflict(
                "Quiz.AlreadyDeleted",
                $"Quiz already deleted");

        public static readonly Error AlreadyPublished =
            Error.Conflict(
                "Quiz.AlreadyPublished",
                $"Quiz already published");

        public static readonly Error AlreadyInDraft =
            Error.Conflict(
                "Quiz.AlreadyInDraft",
                $"Quiz already in draft mode");

        public static readonly Error NoQuestions =
            Error.Conflict(
                "Quiz.NoQuestions",
                $"Quiz has no questions");

        public static Error ReorderCountMismatch(uint expected, uint actual) =>
            Error.Conflict(
                "Quiz.ReorderCountMismatch",
                $"Quiz reordered questions count not equals to actual answers count: expected {expected}, actual {actual}");

        public static readonly Error ReorderMismatch =
            Error.Conflict(
                "Quiz.ReorderMismatch",
                $"Quiz reordered questions contains odd elements or doesn't contains required elements");

        public static readonly Error QuestionsWithLackingAnswersExists =
            Error.Conflict(
                "Quiz.QuestionsWithLackingAnswersExists",
                $"Quiz has questions with only one answer options, minimal is 2");

        public static readonly Error QuestionsWithoutCorrectAnswerExists =
            Error.Conflict(
                "Quiz.QuestionsWithoutCorrectAnswerExists",
                $"Quiz has questions without correct answer");

        public static readonly Error ReorderDuplicatingElements =
            Error.Conflict(
                "Quiz.ReorderDuplicatingElements",
                $"Quiz reordered questions contains duplicating elements");
    }
}
