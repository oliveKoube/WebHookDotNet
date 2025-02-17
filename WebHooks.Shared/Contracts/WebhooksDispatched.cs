namespace WebHooks.Shared.Contracts;

public sealed record WebhooksDispatched(string EventType, object Data);