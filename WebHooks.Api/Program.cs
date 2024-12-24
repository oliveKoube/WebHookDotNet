using Scalar.AspNetCore;
using WebHooks.Api.Models;
using WebHooks.Api.Repository;
using WebHooks.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton<InMemoryOrderRepository>();
builder.Services.AddSingleton<InMemoryWebHookSubscriptionRepository>();

builder.Services.AddHttpClient<WebHookDispatcher>();



var app = builder.Build();

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
}

app.UseHttpsRedirection();

app.MapPost("/webhooks/subscriptions", (CreateWebHookSubscriptionRequest request,
    InMemoryWebHookSubscriptionRepository webHookSubscriptionRepository) =>
{
    WebHookSubscription webHookSubscription = new WebHookSubscription(Guid.NewGuid(),
        request.EventType,
        request.WebHookUrl,
        DateTime.UtcNow);
    webHookSubscriptionRepository.Add(webHookSubscription);
    return Results.Ok(webHookSubscription);
});

app.MapPost("/orders", async (CreateOrderRequest request, InMemoryOrderRepository orderRepository,
        WebHookDispatcher webHookDispatcher) =>
    {
        var order = new Order(Guid.NewGuid(), request.CustomerName, request.Amount, DateTime.UtcNow);

        orderRepository.Add(order);

        await webHookDispatcher.DispatchAsync("OrderCreated", order);
        return Results.Ok(order);

    })
    .WithTags("Orders");

app.MapGet("/orders", (InMemoryOrderRepository orderRepository) =>
    Results.Ok(orderRepository.GetAll())).WithTags("Orders");

app.Run();
