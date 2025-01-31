namespace WebHooks.Api.Services;

internal sealed record WebhooksDispatched(string EventType, object Data);