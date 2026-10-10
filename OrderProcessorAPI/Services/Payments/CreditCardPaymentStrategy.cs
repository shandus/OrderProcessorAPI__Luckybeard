
namespace OrderProcessorAPI.Services.Payments
{
    public class CreditCardPaymentStrategy : IPaymentStrategy
    {
        private readonly ILogger<CreditCardPaymentStrategy> _logger;

        public CreditCardPaymentStrategy(
            ILogger<CreditCardPaymentStrategy> logger)
        {
            _logger = logger;
        }

        public string PaymentMethod => "CreditCard";

        public async Task<bool> ProcessPaymentAsync(decimal amount)
        {
            _logger.LogInformation(
                "Processing mock credit card payment of R{Amount}",
                amount);

            await Task.Delay(200);

            return true;
        }
    }
}
