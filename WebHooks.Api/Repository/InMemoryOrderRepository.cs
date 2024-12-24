using WebHooks.Api.Models;

namespace WebHooks.Api.Repository;

public class InMemoryOrderRepository
{
    private readonly List<Order> _orders = [];
    public void Add(Order order)
    {
        _orders.Add(order);
    }

    public IReadOnlyList<Order> GetAll()
    {
        return _orders.AsReadOnly();
    }
}