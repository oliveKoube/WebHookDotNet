using MassTransit;
using Microsoft.EntityFrameworkCore;
using WebHooks.Shared.Contracts;
using WebHooks.Shared.Data;

namespace WebHooks.Processing.Services;

internal sealed class WebhookDispatchedConsumer(WebhooksDbContext dbContext) : IConsumer<WebhooksDispatched>
{
    public async Task Consume(ConsumeContext<WebhooksDispatched> context)
    {
        var message = context.Message;
        var subscriptions = await dbContext.WebhookSubscriptions
            .AsNoTracking()
            .Where(x => x.EventType == message.EventType)
            .ToListAsync();

        foreach (var webhookSubscription in subscriptions)
            await context.Publish(new WebhookTriggered(webhookSubscription.Id,
                message.EventType,
                webhookSubscription.WebHookUrl,
                message.Data));
    }
}