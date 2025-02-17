using MassTransit;
using MassTransit.Logging;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Scalar.AspNetCore;
using WebHooks.Api.Data;
using WebHooks.Api.Diagnostics;
using WebHooks.Api.Extensions;
using WebHooks.Api.Models;
using WebHooks.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();

builder.Services.AddScoped<WebhookDispatcher>();

builder.Services.AddDbContext<WebhooksDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("webhooks")));

// builder.Services.AddHostedService<WebhooksProcessor>();
// builder.Services.AddSingleton(_ => Channel.CreateBounded<WebhooksDispatch>(new BoundedChannelOptions(100)
// {
//     FullMode = BoundedChannelFullMode.Wait
// }));

builder.Services.AddMassTransit(cfg =>
{
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

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("WebHooks API")
            .WithTheme(ScalarTheme.Mars)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });

    await app.ApplyMigration();
}

app.UseHttpsRedirection();

app.MapPost("/webhooks/subscriptions", async (CreateWebhookSubscriptionRequest request,
    WebhooksDbContext dbContext) =>
{
    var webHookSubscription = new WebhookSubscription(Guid.NewGuid(),
        request.EventType,
        request.WebHookUrl,
        DateTime.UtcNow);
    dbContext.WebhookSubscriptions.Add(webHookSubscription);

    await dbContext.SaveChangesAsync();

    return Results.Ok(webHookSubscription);
});

app.MapPost("/orders", async (CreateOrderRequest request, WebhooksDbContext dbContext,
        WebhookDispatcher webHookDispatcher) =>
    {
        var order = new Order(Guid.NewGuid(), request.CustomerName, request.Amount, DateTime.UtcNow);

        dbContext.Orders.Add(order);

        await dbContext.SaveChangesAsync();

        await webHookDispatcher.DispatchAsync("OrderCreated", order);
        return Results.Ok(order);
    })
    .WithTags("Orders");

app.MapGet("/orders", async (WebhooksDbContext dbContext) =>
        Results.Ok(await dbContext.Orders.ToListAsync()))
    .WithTags("Orders");

app.Run();