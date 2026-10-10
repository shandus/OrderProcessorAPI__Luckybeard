
using OrderProcessorAPI.Models;

namespace OrderProcessorAPI.Repositories
{
    public interface IProductRepository
    {
        Task<List<Product>> GetProductsByIdsAsync(List<int> productIds);
        Task<List<Product>> GetAllProductsAsync();
    }
}
