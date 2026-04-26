using System.Text.Json.Serialization;

using GramQ.Api.Authentication;
using GramQ.QuizManagement.Application;
using GramQ.QuizManagement.Application.Abstractions;
using GramQ.QuizManagement.Infrastructure;

using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
builder.Services.AddQuizManagementApplication().AddQuizManagementInfrastructure(builder.Configuration);
builder.Services.AddOpenApi();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddScoped<ICurrentUserContext, DevelopmentCurrentUserContext>();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();
app.MapControllers();

app.Run();
