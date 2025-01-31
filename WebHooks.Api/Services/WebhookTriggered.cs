namespace WebHooks.Api.Services;

internal sealed record WebhookTriggered(Guid SubscriptionId, string EventType, string WebHookUrl, object Data);