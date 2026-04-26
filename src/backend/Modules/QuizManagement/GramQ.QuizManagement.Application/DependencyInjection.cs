using GramQ.QuizManagement.Application.UseCases.Commands;
using GramQ.QuizManagement.Application.UseCases.Queries;

using Microsoft.Extensions.DependencyInjection;

namespace GramQ.QuizManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddQuizManagementApplication(
        this IServiceCollection services)
    {
        // Commands
        services.AddScoped<AddAnswerOptionCommandHandler>();
        services.AddScoped<AddQuestionCommandHandler>();
        services.AddScoped<CreateQuizCommandHandler>();
        services.AddScoped<DeleteQuizCommandHandler>();
        services.AddScoped<PublishQuizCommandHandler>();
        services.AddScoped<RemoveAnswerOptionCommandHandler>();
        services.AddScoped<RemoveQuestionCommandHandler>();
        services.AddScoped<ReorderAnswersOptionCommandHandler>();
        services.AddScoped<ReorderQuestionsCommandHandler>();
        services.AddScoped<UnpublishQuizCommandHandler>();
        services.AddScoped<UpdateAnswerOptionCommandHandler>();
        services.AddScoped<UpdateQuestionCommandHandler>();
        services.AddScoped<UpdateQuizCommandHandler>();

        // Queries
        services.AddScoped<GetQuizByIdQueryHandler>();
        services.AddScoped<GetQuizzesQueryHandler>();

        return services;
    }
}
