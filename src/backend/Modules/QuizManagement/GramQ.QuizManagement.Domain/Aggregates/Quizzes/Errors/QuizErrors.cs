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
            Error.Validation(
                "Question.AnswerOptionsEmpty",
                $"Question answer options empty, nothing to remove");

        public static readonly Error AlreadyHasCorrectAnswer =
            Error.Validation(
                "Question.AlreadyHasCorrectAnswer",
                $"Question already has correct answer");

        public static readonly Error LastCorrectAnswerDelete =
            Error.Validation(
                "Question.LastCorrectAnswerDelete",
                $"Can't delete last answer that is correct");

        public static Error AnswerOptionsNotFound(Guid answerOptionId) =>
            Error.Validation(
                "Question.AnswerOptionsNotFound",
                $"Question answer option with given id not found: id {answerOptionId}");
    }
}
