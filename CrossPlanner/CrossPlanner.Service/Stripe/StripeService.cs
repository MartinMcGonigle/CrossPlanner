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

            var customers = await customerService.ListAsync(listOptions, requestOptions);

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

                var newCustomer = await customerService.CreateAsync(customerOptions, requestOptions);

                return newCustomer;
            }
        }

        public async Task<(bool Success, string Message, string PaymentId)> ProcessPayment(decimal amount, string customerId, string connectedAccountId)
        {
            var chargeOptions = new ChargeCreateOptions
            {
                Amount = (long)(amount * 100),
                Currency = "gbp",
                Description = "Membership Fee",
                Customer = customerId,
            };

            var requestOptions = new RequestOptions
            {
                StripeAccount = connectedAccountId
            };

            var service = new ChargeService();

            try
            {
                var charge = await service.CreateAsync(chargeOptions, requestOptions);
                bool success = charge.Status == "succeeded";
                return (success, charge.Status, success ? charge.Id : null);
            }
            catch (StripeException ex)
            {
                return (false, ex.Message, null);
            }
        }

        public async Task<(bool Success, string Message)> RefundCustomer(decimal refundAmount, string lastPaymentId)
        {
            var refundOptions = new RefundCreateOptions
            {
                Amount = (long)(refundAmount * 100),
                Charge = lastPaymentId
            };

            var refundService = new RefundService();

            try
            {
                var refund = await refundService.CreateAsync(refundOptions);
                return (refund.Status == "succeeded", refund.Status);
            }
            catch (StripeException ex)
            {
                return (false, ex.Message);
            }
        }
    }
}