
using Microsoft.AspNetCore.Mvc;
using OrderProcessorAPI.Services;
using OrderProcessorAPI.Services.Payments;
using System.ComponentModel.DataAnnotations;

namespace OrderProcessorAPI.Controllers
{
    [ApiController]
    [Route("api/mock")]
    public class MockController : ControllerBase
    {
        private readonly PaymentStrategyFactory _paymentFactory;
        private readonly IEmailSender _emailSender;

        public MockController(PaymentStrategyFactory paymentFactory, IEmailSender emailSender)
        {
            _paymentFactory = paymentFactory;
            _emailSender = emailSender;
        }

        [HttpPost("payment")]
        public async Task<IActionResult> ProcessPayment(
            [FromBody] MockPaymentRequest request)
        {
            if (request.Amount <= 0)
            {
                return BadRequest("Amount must be greater than zero.");
            }

            IPaymentStrategy strategy;

            try
            {
                strategy = _paymentFactory.GetStrategy(
                    request.PaymentMethod);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }

            var success = await strategy.ProcessPaymentAsync(
                request.Amount);

            return Ok(new
            {
                PaymentMethod = strategy.PaymentMethod,
                Amount = request.Amount,
                Currency = "ZAR",
                Success = success,
                Message = success
                    ? "Mock payment processed successfully"
                    : "Mock payment failed"
            });
        }


        [HttpPost("email")]
        public async Task<IActionResult> SendMockEmail(
            [FromBody] MockEmailRequest request)
        {
            await _emailSender.SendOrderConfirmationAsync(
                request.CustomerEmail,
                request.OrderId,
                request.Total);

            return Ok(new
            {
                Success = true,
                Message = "Mock email sent successfully",
                OrderId = request.OrderId,
                CustomerEmail = request.CustomerEmail
            });
        }


    }

    public class MockPaymentRequest
    {
        [Required]
        public string PaymentMethod { get; set; } = string.Empty;

        [Range(
            typeof(decimal),
            "0.01",
            "999999999",
            ParseLimitsInInvariantCulture = true)]
        public decimal Amount { get; set; }
    }


    public class MockEmailRequest
    {
        [Required]
        [EmailAddress]
        public string CustomerEmail { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int OrderId { get; set; }

        [Range(
            typeof(decimal),
            "0.01",
            "999999999",
            ParseLimitsInInvariantCulture = true)]
        public decimal Total { get; set; }
    }

}
