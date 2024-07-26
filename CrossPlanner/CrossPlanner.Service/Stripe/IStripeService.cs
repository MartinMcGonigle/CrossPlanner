using Stripe;

namespace CrossPlanner.Service.Stripe
{

    public interface IStripeService
    {
        public Task<Customer> GetOrCreateCustomer(string email, string sourceToken, string connectedAccountId);
    }
}