using WebHooks.Api.Repository;

namespace WebHooks.Api.Services;

internal sealed class WebHookDispatcher(HttpClient httpClient,
    InMemoryWebHookSubscriptionRepository webHookSubscriptionRepository)
{
    public async Task DispatchAsync(string eventType, object payload)
    {
        var subscriptions = webHookSubscriptionRepository.GetByEventType(eventType);

        foreach (var subscription in subscriptions)
        {
            var request = new
            {
                Id = Guid.NewGuid(),
                EventType = eventType,
                SubscriptionId = subscription.Id,
                Timestamp = DateTime.UtcNow,
                Data = payload
            };
            await httpClient.PostAsJsonAsync(subscription.WebHookUrl, request);
        }
    }
}