
using OrderProcessorAPI.Models;

namespace OrderProcessorAPI.Repositories
{
    public interface IOrderRepository
    {
        Task AddOrderAsync(Order order);
        Task<List<Order>> GetOrdersByCustomerIdAsync(int customerId);
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task SaveChangesAsync();
    }
}
