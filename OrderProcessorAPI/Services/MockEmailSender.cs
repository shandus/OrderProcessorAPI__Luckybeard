
namespace OrderProcessorAPI.Services
{
    public class MockEmailSender : IEmailSender
    {
        private readonly ILogger<MockEmailSender> _logger;

        public MockEmailSender(ILogger<MockEmailSender> logger)
        {
            _logger = logger;
        }

        public async Task SendOrderConfirmationAsync(
            string customerEmail,
            int orderId,
            decimal total)
        {
            _logger.LogInformation(
                "Sending mock order confirmation. " +
                "Email: {Email}, OrderId: {OrderId}, Total: R{Total}",
                customerEmail,
                orderId,
                total);

            // simulate sending an email
            await Task.Delay(200);

            _logger.LogInformation(
                "Mock email sent successfully for Order {OrderId}",
                orderId);
        }
    }
}
