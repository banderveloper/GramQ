namespace GramQ.QuizManagement.Domain.Aggregates.Quizzes;

public static class QuizRules
{
    public const uint MaxAnswerOptionsPerQuestion = 4;
    public const uint MinAnswerOptionsPerQuestion = 2;
    public const uint MaxQuestionsPerQuiz = 50;
    public const uint MinTimeLimitSeconds = 5;
    public const uint MaxTimeLimitSeconds = 120;
    public const int MaxQuestionTextLength = 500;
    public const int MaxAnswerOptionTextLength = 200;
    public const int MaxQuizTitleLength = 100;
}
