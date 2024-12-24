using WebHooks.Api.Models;

namespace WebHooks.Api.Repository;

public class InMemoryWebHookSubscriptionRepository
{
    private readonly List<WebHookSubscription> _webHookSubscriptions = [];
    public void Add(WebHookSubscription order)
    {
        _webHookSubscriptions.Add(order);
    }

    public IReadOnlyList<WebHookSubscription> GetByEventType(string EventType)
    {
        return _webHookSubscriptions.Where(x => x.EventType==EventType).ToList().AsReadOnly();
    }
}