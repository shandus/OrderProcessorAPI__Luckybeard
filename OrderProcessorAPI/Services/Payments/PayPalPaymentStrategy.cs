
namespace OrderProcessorAPI.Services.Payments
{
    public class PayPalPaymentStrategy : IPaymentStrategy
    {
        private readonly ILogger<PayPalPaymentStrategy> _logger;

        public PayPalPaymentStrategy(
            ILogger<PayPalPaymentStrategy> logger)
        {
            _logger = logger;
        }

        public string PaymentMethod => "PayPal";

        public async Task<bool> ProcessPaymentAsync(decimal amount)
        {
            _logger.LogInformation(
                "Processing mock PayPal payment of R{Amount}",
                amount);

            await Task.Delay(200);

            return true;
        }
    }
}
