namespace WebHooks.Api.Models;

public sealed record Order(Guid Id, string CustomerName, decimal Amount, DateTime CreateAt);

public sealed record CreateOrderRequest(string CustomerName, decimal Amount);