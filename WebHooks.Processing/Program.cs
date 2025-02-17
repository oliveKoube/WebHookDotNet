using MassTransit;
using MassTransit.Logging;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using WebHooks.Processing.Diagnostics;
using WebHooks.Processing.Services;
using WebHooks.Shared.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<WebhooksDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("webhooks")));

builder.Services.AddMassTransit(cfg =>
{
    cfg.AddConsumer<WebhookTriggeredConsumer>();
    cfg.AddConsumer<WebhookDispatchedConsumer>();

    cfg.SetKebabCaseEndpointNameFormatter();
    cfg.UsingRabbitMq((context, config) =>
    {
        config.Host(builder.Configuration.GetConnectionString("rabbitmq"));
        config.ConfigureEndpoints(context);
    });
});

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .AddSource(DiagnosticsConfig.ActivitySource.Name)
            .AddNpgsql()
            .AddSource(DiagnosticHeaders.DefaultListenerName);
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();