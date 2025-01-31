using System.Text.Json;
using MassTransit;
using Webhooks.Api.Data;
using WebHooks.Api.Models;

namespace WebHooks.Api.Services;

internal sealed class WebhookTriggeredConsumer(WebhooksDbContext dbContext, IHttpClientFactory httpClientFactory)
    : IConsumer<WebhookTriggered>
{
    public async Task Consume(ConsumeContext<WebhookTriggered> context)
    {
        var message = context.Message;

        using var httpClient = httpClientFactory.CreateClient();
        var payload = new WebhooksPayload
        {
            Id = Guid.NewGuid(),
            EventType = message.EventType,
            SubscriptionId = message.SubscriptionId,
            Timestamp = DateTime.UtcNow,
            Data = message.Data
        };

        var jsonPayload = JsonSerializer.Serialize(payload);
        try
        {
            var response = await httpClient.PostAsJsonAsync(message.WebHookUrl, payload);
            response.EnsureSuccessStatusCode();

            var attempt = new WebhookDeliveryAttempt
            {
                Id = Guid.NewGuid(),
                WebhookSubscriptionId = message.SubscriptionId,
                ResponseStatusCode = (int)response.StatusCode,
                Payload = jsonPayload,
                Success = response.IsSuccessStatusCode,
                Timestamp = DateTime.UtcNow
            };

            await dbContext.WebhookDeliveryAttempts.AddAsync(attempt);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            var attempt = new WebhookDeliveryAttempt
            {
                Id = Guid.NewGuid(),
                WebhookSubscriptionId = message.SubscriptionId,
                ResponseStatusCode = null,
                Payload = jsonPayload,
                Success = false,
                Timestamp = DateTime.UtcNow
            };

            await dbContext.WebhookDeliveryAttempts.AddAsync(attempt);
            await dbContext.SaveChangesAsync();
        }
    }
}