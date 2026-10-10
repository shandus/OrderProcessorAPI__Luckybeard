
namespace OrderProcessorAPI.Services.Payments
{
    public class PaymentStrategyFactory
    {
        private readonly IEnumerable<IPaymentStrategy> _strategies;

        public PaymentStrategyFactory(
            IEnumerable<IPaymentStrategy> strategies)
        {
            _strategies = strategies;
        }

        public IPaymentStrategy GetStrategy(string paymentMethod)
        {
            var strategy = _strategies.FirstOrDefault(s =>
                s.PaymentMethod.Equals(
                    paymentMethod,
                    StringComparison.OrdinalIgnoreCase));

            if (strategy == null)
            {
                throw new ArgumentException(
                    $"Unsupported payment method: {paymentMethod}");
            }

            return strategy;
        }
    }
}
