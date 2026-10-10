
using OrderProcessorAPI.Models;

namespace OrderProcessorAPI.Services
{
    public interface IOrderProcessor
    {
        Task<OrderResponse> ProcessOrderAsync(
            ProcessRequest request);
    }
}
