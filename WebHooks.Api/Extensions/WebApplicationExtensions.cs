using Microsoft.EntityFrameworkCore;
using Webhooks.Api.Data;

namespace WebHooks.Api.Extensions;

public static class WebApplicationExtensions
{
    public static async Task ApplyMigration(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WebhooksDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}