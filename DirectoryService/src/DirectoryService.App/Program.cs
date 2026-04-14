using DirectoryService.App.Extensions;
using DirectoryService.App.Middlewares;
using DirectoryService.Application;
using DirectoryService.Infrastructure.Postgres;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddGlobalExceptionHandling();

builder.Services.AddDatabaseConfiguration(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddApiVersioningSupport();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerConfiguration();

builder.Host.AddLogging(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerConfiguration();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();