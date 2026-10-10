
namespace OrderProcessorAPI.Services.Payments
{
    public class BankTransferPaymentStrategy : IPaymentStrategy
    {
        private readonly ILogger<BankTransferPaymentStrategy> _logger;

        public BankTransferPaymentStrategy(
            ILogger<BankTransferPaymentStrategy> logger)
        {
            _logger = logger;
        }

        public string PaymentMethod => "BankTransfer";

        public async Task<bool> ProcessPaymentAsync(decimal amount)
        {
            _logger.LogInformation(
                "Simulating bank transfer request of R{Amount}",
                amount);

            await Task.Delay(200);

            return true;
        }
    }
}
