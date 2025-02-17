namespace WebHooks.Shared.Models;

public sealed record WebhookSubscription(Guid Id, string EventType, string WebHookUrl, DateTime CreatedOnUtc);

public sealed record CreateWebhookSubscriptionRequest(string EventType, string WebHookUrl);