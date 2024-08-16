using Stripe;

namespace CrossPlanner.Service.Stripe
{

    public interface IStripeService
    {
        public Task<Customer> GetOrCreateCustomer(string email, string sourceToken, string connectedAccountId);

        public Task<(bool Success, string Message, string PaymentId)> ProcessPayment(decimal amount, string customerId, string connectedAccountId);

        public Task<(bool Success, string Message, string RefundId)> RefundCustomer(decimal refundAmount, string lastPaymentId, string connectedAccountId);
    }
}