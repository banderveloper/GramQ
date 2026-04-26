using GramQ.QuizManagement.Application.Abstractions;
using GramQ.QuizManagement.Infrastructure.Persistence.DbContext;
using GramQ.QuizManagement.Infrastructure.Persistence.Interceptors;
using GramQ.QuizManagement.Infrastructure.Persistence.Repositories;
using GramQ.QuizManagement.Infrastructure.Services;
using GramQ.Shared.Abstractions.Time;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GramQ.QuizManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddQuizManagementInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var dbOptions = configuration
                            .GetSection(QuizManagementDbOptions.SectionName)
                            .Get<QuizManagementDbOptions>()
                        ?? throw new InvalidOperationException("QuizManagementDb configuration is missing.");

        // DbContext
        services.AddScoped<ConvertDomainEventsToOutboxMessagesInterceptor>();

        services.AddDbContext<QuizManagementDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(dbOptions.ConnectionString);
            options.AddInterceptors(
                serviceProvider.GetRequiredService<ConvertDomainEventsToOutboxMessagesInterceptor>());
        });

        // Repository
        services.AddScoped<IQuizRepository, QuizRepository>();

        // UnitOfWork
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<QuizManagementDbContext>());

        // DateTimeProvider
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        return services;
    }
}
