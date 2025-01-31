using MassTransit;
using WebHooks.Api.Diagnostics;

namespace WebHooks.Api.Services;

internal sealed class WebhookDispatcher(
    IPublishEndpoint publishEndpoint)
{
    public async Task DispatchAsync<T>(string eventType, T data)
        where T : notnull
    {
        using var activity = DiagnosticsConfig.ActivitySource.StartActivity($"{eventType} dispatch webhook");
        activity?.SetTag("event.Type", eventType);
        await publishEndpoint.Publish(new WebhooksDispatched(eventType, data));
    }
}