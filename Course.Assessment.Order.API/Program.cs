using Course.Assessment.Order.API.Endpoints;
using Course.Assessment.Order.API.Extensions;
using Course.Assessment.Order.Application;
using Course.Assessment.Order.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
    app.ApplyMigrations();
}

app.UseHttpsRedirection();
app.MapOrderEndpoints();
app.Run();
