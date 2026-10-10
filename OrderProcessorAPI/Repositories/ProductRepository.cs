
using Microsoft.EntityFrameworkCore;
using OrderProcessorAPI.Data;
using OrderProcessorAPI.Models;

namespace OrderProcessorAPI.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly OrderDbContext _context;

        public ProductRepository(OrderDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetProductsByIdsAsync(
            List<int> productIds)
        {
            return await _context.Products
                .AsNoTracking()
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await _context.Products
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .ToListAsync();
        }
    }
}
