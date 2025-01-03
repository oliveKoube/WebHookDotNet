using Microsoft.EntityFrameworkCore;
using WebHooks.Api.Models;

namespace Webhooks.Api.Data;

internal sealed class WebhooksDbContext(DbContextOptions<WebhooksDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    
    public DbSet<WebhookSubscription> WebhookSubscriptions => Set<WebhookSubscription>();
    
    public DbSet<WebhookDeliveryAttempt> WebhookDeliveryAttempts => Set<WebhookDeliveryAttempt>();
    
    override protected void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(builder =>
        {
            builder.ToTable("orders");
            builder.HasKey(x => x.Id);
        });
        
        modelBuilder.Entity<WebhookSubscription>(builder =>
        {
            builder.ToTable("subscriptions", "webhooks");
            builder.HasKey(x => x.Id);
        });
        
        modelBuilder.Entity<WebhookDeliveryAttempt>(builder =>
        {
            builder.ToTable("delivery_attempts", "webhooks");
            builder.HasKey(x => x.Id);
            
            builder.HasOne<WebhookSubscription>()
                .WithMany()
                .HasForeignKey(x => x.WebhookSubscriptionId);
        });
    }
}