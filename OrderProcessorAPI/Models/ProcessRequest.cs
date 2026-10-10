
using System.ComponentModel.DataAnnotations;

namespace OrderProcessorAPI.Models
{
    public class ProcessRequest
    {
        [Range(1, int.MaxValue)]
        public int CustomerId { get; set; }

        [Required]
        [EmailAddress]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required]
        [MinLength(1)]
        public List<OrderItemRequest> Items { get; set; } = new();

        [Required]
        public string PaymentMethod { get; set; } = string.Empty;

        [Range(0, 100)]
        public decimal Discount { get; set; }
    }

    public class OrderItemRequest
    {
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }

        [Range(
            typeof(decimal),
            "0.01",
            "10000",
            ParseLimitsInInvariantCulture = true,
            ErrorMessage = "Quantity must be between 0.01 and 10000.")]
        public decimal Quantity { get; set; }
    }
}
