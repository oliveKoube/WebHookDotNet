namespace WebHooks.Api.Models;

public sealed record WebHookSubscription(Guid Id, string EventType, string WebHookUrl,DateTime CreatedOnUtc);

public sealed record CreateWebHookSubscriptionRequest(string EventType, string WebHookUrl);