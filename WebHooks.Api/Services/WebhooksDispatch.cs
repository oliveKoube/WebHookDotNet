namespace WebHooks.Api.Services;

internal sealed record WebhooksDispatch(string EventType, object Data, string ParentActivityId);