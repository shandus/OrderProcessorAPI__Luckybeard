
namespace OrderProcessorAPI.Services.Payments
{
    public interface IPaymentStrategy
    {
        string PaymentMethod { get; }

        Task<bool> ProcessPaymentAsync(decimal amount);
    }
}
