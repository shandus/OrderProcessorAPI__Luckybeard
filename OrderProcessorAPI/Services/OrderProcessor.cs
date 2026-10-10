
using Microsoft.EntityFrameworkCore;
using OrderProcessorAPI.Data;
using OrderProcessorAPI.Models;
using OrderProcessorAPI.Repositories;
using OrderProcessorAPI.Services.Payments;

namespace OrderProcessorAPI.Services
{
    public class OrderProcessor : IOrderProcessor
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly PaymentStrategyFactory _paymentFactory;
        private readonly IEmailSender _emailSender;
        private readonly OrderDbContext _context;
        private readonly ILogger<OrderProcessor> _logger;

        public OrderProcessor(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            PaymentStrategyFactory paymentFactory,
            IEmailSender emailSender,
            OrderDbContext context,
            ILogger<OrderProcessor> logger)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _paymentFactory = paymentFactory;
            _emailSender = emailSender;
            _context = context;
            _logger = logger;
        }


        public async Task<OrderResponse> ProcessOrderAsync(ProcessRequest request)
        {
            // 1. Validate business rules
            if (request == null)
            {
                throw new ArgumentException("Order request cannot be null.");
            }

            if (request.CustomerId <= 0)
            {
                throw new ArgumentException("Invalid customer ID.");
            }

            if (string.IsNullOrWhiteSpace(request.CustomerEmail))
            {
                throw new ArgumentException("Customer email is required.");
            }

            if (request.Items == null || !request.Items.Any())
            {
                throw new ArgumentException(
                    "Order must contain at least one item.");
            }

            if (request.Items.Any(i => i == null ||
                                       i.ProductId <= 0 ||
                                       i.Quantity <= 0))
            {
                throw new ArgumentException(
                    "Products and quantities must be valid.");
            }

            if (request.Discount < 0 || request.Discount > 100)
            {
                throw new ArgumentException(
                    "Discount must be between 0 and 100.");
            }

            // 2. Retrieve products from database
            var productIds = request.Items
                .Select(i => i.ProductId)
                .Distinct()
                .ToList();

            var products = await _productRepository
                .GetProductsByIdsAsync(productIds);

            if (products.Count != productIds.Count)
            {
                throw new ArgumentException(
                    "One or more products do not exist.");
            }

            // 3. Calculate order total
            decimal subtotal = 0;
            var orderItems = new List<OrderItem>();

            foreach (var item in request.Items)
            {
                var product = products.First(
                    p => p.Id == item.ProductId);

                decimal lineTotal = product.Price * item.Quantity;

                subtotal += lineTotal;

                orderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price
                });
            }

            // Apply percentage discount
            decimal discountAmount =
                subtotal * request.Discount / 100m;

            decimal total = Math.Round(
                subtotal - discountAmount,
                2,
                MidpointRounding.AwayFromZero);

            if (total <= 0)
            {
                throw new ArgumentException(
                    "Order total must be greater than zero.");
            }

            _logger.LogInformation(
                "Processing order for customer {CustomerId}. Total: R{Total}",
                request.CustomerId,
                total);

            // 4. Select payment strategy
            var paymentStrategy = _paymentFactory
                .GetStrategy(request.PaymentMethod);

            // 5. Process mock payment
            bool paymentSuccessful = await paymentStrategy
                .ProcessPaymentAsync(total);

            if (!paymentSuccessful)
            {
                _logger.LogWarning(
                    "Payment failed for customer {CustomerId}",
                    request.CustomerId);

                return new OrderResponse
                {
                    CustomerId = request.CustomerId,
                    Total = total,
                    Status = "PaymentFailed",
                    Message = "Payment was unsuccessful."
                };
            }

            // 6. Create order
            var order = new Order
            {
                CustomerId = request.CustomerId,
                Total = total,
                PaymentMethod = paymentStrategy.PaymentMethod,
                Status = "Paid",
                CreatedAt = DateTime.UtcNow,
                Items = orderItems
            };

            // 7. Save order using database transaction
            await using (var transaction =
                await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    await _orderRepository.AddOrderAsync(order);

                    await _orderRepository.SaveChangesAsync();

                    await transaction.CommitAsync();

                    _logger.LogInformation(
                        "Order {OrderId} saved successfully.",
                        order.Id);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    _logger.LogError(
                        ex,
                        "Failed to save order for customer {CustomerId}",
                        request.CustomerId);

                    throw;
                }
            }

            // 8. Send order confirmation email
            try
            {
                await _emailSender.SendOrderConfirmationAsync(
                    request.CustomerEmail,
                    order.Id,
                    order.Total);
            }
            catch (Exception ex)
            {
                // Email failure should not reverse a successful order
                _logger.LogError(
                    ex,
                    "Failed to send confirmation email for order {OrderId}",
                    order.Id);
            }

            // 9. Return successful response
            return new OrderResponse
            {
                OrderId = order.Id,
                CustomerId = order.CustomerId,
                Total = order.Total,
                Status = order.Status,
                Message = "Order processed successfully."
            };
        }

    }
}
