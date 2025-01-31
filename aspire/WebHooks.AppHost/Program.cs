using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var database = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin()
    .AddDatabase("webhooks");

var rabbitMq = builder.AddRabbitMQ("rabbitmq")
    .WithDataVolume()
    .WithManagementPlugin();

builder.AddProject<WebHooks_Api>("webhooks-api")
    .WithReference(database)
    .WithReference(rabbitMq)
    .WaitFor(database)
    .WaitFor(rabbitMq);

builder.Build()
    .Run();