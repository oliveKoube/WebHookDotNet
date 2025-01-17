using System.Diagnostics;
using System.Threading.Channels;
using WebHooks.Api.Diagnostics;

namespace WebHooks.Api.Services;

internal sealed class WebhooksProcessor(
    IServiceScopeFactory serviceScopeFactory,
    Channel<WebhooksDispatch> webhooksDispatch) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach(WebhooksDispatch dispatch in webhooksDispatch.Reader.ReadAllAsync(stoppingToken))
        {
            using var activity = DiagnosticsConfig.ActivitySource.StartActivity($"{dispatch.EventType} process webhook",
                ActivityKind.Internal, parentId: dispatch.ParentActivityId);

            using var scope = serviceScopeFactory.CreateScope();

            var dispatcher = scope.ServiceProvider.GetRequiredService<WebhookDispatcher>();

            await dispatcher.ProcessAsync(dispatch.EventType, dispatch.Data);
        }
    }
}