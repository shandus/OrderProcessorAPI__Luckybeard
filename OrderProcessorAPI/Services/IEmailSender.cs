
namespace OrderProcessorAPI.Services
{
    public interface IEmailSender
    {
        Task SendOrderConfirmationAsync(
            string customerEmail,
            int orderId,
            decimal total);
    }
}
