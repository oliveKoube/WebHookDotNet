using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Webhooks.Api.Data;
using WebHooks.Api.Extensions;
using WebHooks.Api.Models;
using WebHooks.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();

builder.Services.AddTransient<WebhookDispatcher>();

builder.Services.AddDbContext<WebhooksDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("webhooks")));



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
    WebhookSubscription webHookSubscription = new WebhookSubscription(Guid.NewGuid(),
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
    Results.Ok(await dbContext.Orders.ToListAsync())).WithTags("Orders");

app.Run();
