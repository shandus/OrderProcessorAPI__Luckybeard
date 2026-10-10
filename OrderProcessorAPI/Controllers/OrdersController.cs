
using Microsoft.AspNetCore.Mvc;
using OrderProcessorAPI.Models;
using OrderProcessorAPI.Repositories;
using OrderProcessorAPI.Services;

namespace OrderProcessorAPI.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderProcessor _orderProcessor;
        private readonly IOrderRepository _orderRepository;

        public OrdersController(
            IOrderProcessor orderProcessor,
            IOrderRepository orderRepository)
        {
            _orderProcessor = orderProcessor;
            _orderRepository = orderRepository;
        }

        // replacement for DoProcess()

        [HttpPost]
        public async Task<IActionResult> CreateOrder(
            [FromBody] ProcessRequest request)
        {
            var response = await _orderProcessor.ProcessOrderAsync(request);

            if (response.Status == "PaymentFailed")
                return BadRequest(response);

            return Ok(response);
        }


        // replacement for FindHistory()
        [HttpGet("customer/{customerId:int}")]
        public async Task<IActionResult> GetOrderHistory(
            int customerId)
        {
            if (customerId <= 0)
                return BadRequest("Invalid customer ID.");

            var orders = await _orderRepository
                .GetOrdersByCustomerIdAsync(customerId);

            return Ok(orders.Select(o => new
            {
                o.Id,
                o.CustomerId,
                o.Total,
                o.Status,
                o.PaymentMethod,
                o.CreatedAt,
                Items = o.Items.Select(i => new
                {
                    i.ProductId,
                    ProductName = i.Product.Name,
                    i.Quantity,
                    i.UnitPrice,
                    LineTotal = i.Quantity * i.UnitPrice
                })
            }));
        }

        [HttpGet("{orderId:int}")]
        public async Task<IActionResult> GetOrder(int orderId)
        {
            var order = await _orderRepository
                .GetOrderByIdAsync(orderId);

            if (order == null)
                return NotFound("Order not found.");

            return Ok(new
            {
                order.Id,
                order.CustomerId,
                order.Total,
                order.Status,
                order.PaymentMethod,
                order.CreatedAt,
                Items = order.Items.Select(i => new
                {
                    i.ProductId,
                    ProductName = i.Product.Name,
                    i.Quantity,
                    i.UnitPrice,
                    LineTotal = i.Quantity * i.UnitPrice
                })
            });
        }
    }
}
