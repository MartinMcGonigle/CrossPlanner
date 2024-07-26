using Stripe;

namespace CrossPlanner.Service.Stripe
{
    public class StripeService : IStripeService
    {
        public async Task<Customer> GetOrCreateCustomer(string email, string sourceToken, string connectedAccountId)
        {
            var customerService = new CustomerService();

            var listOptions = new CustomerListOptions
            {
                Email = email,
                Limit = 1
            };

            var requestOptions = new RequestOptions
            {
                StripeAccount = connectedAccountId
            };

            StripeList<Customer> customers = await customerService.ListAsync(listOptions, requestOptions);

            // Check if we have any customers with that email
            if (customers != null && customers.Data.Count > 0)
            {
                return customers.Data.First();
            }
            else
            {
                var customerOptions = new CustomerCreateOptions
                {
                    Email = email,
                    Source = sourceToken
                };

                Customer newCustomer = await customerService.CreateAsync(customerOptions, requestOptions);

                return newCustomer;
            }
        }
    }
}