namespace WebHooks.Shared.Contracts;

public sealed record WebhookTriggered(Guid SubscriptionId, string EventType, string WebHookUrl, object Data);