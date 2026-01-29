using Course.Assessment.Order.API.Endpoints;
using Course.Assessment.Payment.API.Extensions;
using Course.Assessment.Payment.Application;
using Course.Assessment.Payment.Infrastructure;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddSingleton<IConnection>(sp =>
{
    var factory = new ConnectionFactory
    {
        HostName = "RabbitMq"
    };
    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
});

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
app.MapPaymentEndpoints();
app.Run();
