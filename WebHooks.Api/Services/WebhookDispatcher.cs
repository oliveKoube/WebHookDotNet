using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Webhooks.Api.Data;
using WebHooks.Api.Models;

namespace WebHooks.Api.Services;

internal sealed class WebhookDispatcher(IHttpClientFactory httpClientFactory,
    WebhooksDbContext dbContext)
{
    public async Task DispatchAsync<T>(string eventType, T data)
    {
        var subscriptions = await dbContext.WebhookSubscriptions
            .AsNoTracking()
            .Where(x => x.EventType == eventType)
            .ToListAsync();

        foreach (var subscription in subscriptions)
        {
            using var httpClient = httpClientFactory.CreateClient();
            var payload = new WebhooksPayload<T>
            {
                Id = Guid.NewGuid(), 
                EventType = eventType, 
                SubscriptionId = subscription.Id,
                Timestamp = DateTime.UtcNow,    
                Data = data
            };
            
            var jsonPayload = JsonSerializer.Serialize(payload);
            try
            {
                HttpResponseMessage response = await httpClient.PostAsJsonAsync(subscription.WebHookUrl, payload);
                
                var attempt = new WebhookDeliveryAttempt
                {
                    Id = Guid.NewGuid(), 
                    WebhookSubscriptionId = subscription.Id, 
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
                    WebhookSubscriptionId = subscription.Id, 
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
}