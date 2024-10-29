using CrossPlanner.Domain.Context;
using CrossPlanner.Domain.Enums;
using CrossPlanner.Domain.Models;
using CrossPlanner.Repository.Wrapper;
using CrossPlanner.Service.Stripe;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Stripe;

namespace CrossPlanner.MembershipManager
{
    public class Program
    {
        private static IConfiguration configuration;

        public static void Main(string[] args)
        {
            ServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            SetUpLogger();

            Log.Information($"Started membership manager - {DateTime.Now}");

            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            try
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var repositoryWrapper = scope.ServiceProvider.GetRequiredService<IRepositoryWrapper>();
                    var stripeService = scope.ServiceProvider.GetRequiredService<IStripeService>();
                    ProcessExpiredMemberships(repositoryWrapper, stripeService);
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Error running service");
            }
            finally
            {
                Log.CloseAndFlush();
            }

            Environment.Exit(-1);
        }

        private static void ProcessExpiredMemberships(IRepositoryWrapper repositoryWrapper, IStripeService stripeService)
        {
            var dateTimeNow = DateTime.Now;
            var expiredMemberships = repositoryWrapper.MembershipRepository
                .FindByCondition(m => m.IsActive && m.EndDate < dateTimeNow)
                .Include(m => m.MembershipPlan)
                .ToList();

            foreach (var membership in expiredMemberships)
            {
                if (membership.AutoRenew)
                {
                    RenewMembership(membership, repositoryWrapper, stripeService);
                }
                else
                {
                    repositoryWrapper.ScheduledClassReservationRepository.DeleteFutureScheduledClassReservations(membership.MembershipId, DateTime.Now);
                }

                membership.IsActive = false;
                Log.Information($"Deactivated membership {membership.MembershipId} for user {membership.MemberId} due to expiration.");
                repositoryWrapper.MembershipRepository.Update(membership);
                repositoryWrapper.Save();
            }

            Log.Information($"Completed processing expired memberships. {expiredMemberships.Count()} in total.");
        }

        private static void RenewMembership(Membership membership, IRepositoryWrapper repositoryWrapper, IStripeService stripeService)
        {
            try
            {
                var affiliate = repositoryWrapper.AffiliateRepository
                    .FindByCondition(a => a.AffiliateId == membership.MembershipPlan.AffiliateId)
                    .FirstOrDefault();

                if (affiliate == null || string.IsNullOrEmpty(affiliate.ConnectedAccountId))
                {
                    Log.Error($"Missing or invalid affiliate details for membership {membership.MembershipId}");
                    CleanupReservationsOnFailure(repositoryWrapper, membership.MembershipId);
                    return;
                }

                var userStripeAffiliate = repositoryWrapper.UserStripeAffiliateRepository
                    .FindByCondition(usa => usa.ApplicationUserId == membership.MemberId
                    && usa.AffiliateId == affiliate.AffiliateId)
                    .FirstOrDefault();

                if (userStripeAffiliate == null)
                {
                    Log.Error($"Stripe customer ID not found for user {membership.MemberId} in affiliate {affiliate.AffiliateId}");
                    CleanupReservationsOnFailure(repositoryWrapper, membership.MembershipId);
                    return;
                }

                var processPayment = stripeService.ProcessPayment(
                    membership.MembershipPlan.Price, userStripeAffiliate.StripeCustomerId, affiliate.ConnectedAccountId).Result;

                if (processPayment.Success)
                {
                    var endDate = CalculateEndDate(membership);
                    var newMembership = CreateNewMembership(membership, endDate, processPayment.PaymentId, membership.MembershipPlan.Price);
                    repositoryWrapper.MembershipRepository.Create(newMembership);
                    repositoryWrapper.Save();
                    repositoryWrapper.ScheduledClassReservationRepository.UpdateFutureScheduledClassReservationMembershipId(membership.MembershipId, newMembership.MembershipId, DateTime.Now);
                }
                else
                {
                    Log.Error($"Payment failed for membership {membership.MembershipId}, user {membership.MemberId}. Message: {processPayment.Message}");
                    CleanupReservationsOnFailure(repositoryWrapper, membership.MembershipId);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Exception in renewing membership {membership.MembershipId} for user {membership.MemberId}: {ex.Message}");
                CleanupReservationsOnFailure(repositoryWrapper, membership.MembershipId);
            }
        }

        private static void CleanupReservationsOnFailure(IRepositoryWrapper repositoryWrapper, int membershipId)
        {
            repositoryWrapper.ScheduledClassReservationRepository.DeleteFutureScheduledClassReservations(membershipId, DateTime.Now);
        }

        private static Membership CreateNewMembership(Membership oldMembership, DateTime endDate, string paymentId, decimal amount)
        {
            return new Membership
            {
                IsActive = true,
                StartDate = DateTime.Today,
                EndDate = endDate,
                AutoRenew = oldMembership.AutoRenew,
                MemberId = oldMembership.MemberId,
                MembershipPlanId = oldMembership.MembershipPlanId,
                LastPaymentId = paymentId,
                PaymentStatus = PaymentStatus.Paid,
                LastPaymentAmount = amount
            };
        }

        private static DateTime CalculateEndDate(Membership membership)
        {
            DateTime endDate = DateTime.Today.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59);

            if (membership.MembershipPlan.Type == (int)MembershipType.Monthly)
            {
                endDate = DateTime.Today.AddMonths((int)membership.MembershipPlan.NumberOfMonths).AddDays(-1).AddHours(23).AddMinutes(59);
            }

            return endDate;
        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton(LoggerFactory.Create(builder =>
            {
                builder.AddSerilog(dispose: true);
            }));

            serviceCollection.AddLogging();

            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", optional: false)
                .AddUserSecrets<Program>()
                .Build();

            StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];

            serviceCollection.AddDbContext<ApplicationContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("CrossPlannerConnection")));

            serviceCollection.AddSingleton<IConfiguration>(configuration);
            serviceCollection.AddSingleton<IRepositoryWrapper, RepositoryWrapper>();
            serviceCollection.AddSingleton<IStripeService, StripeService>();
        }

        private static void SetUpLogger()
        {
            string file = configuration.GetSection("Logging").GetSection("File").Value;
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(path: $"{file}-{DateTime.Now.Year} -{DateTime.Now.Month}-{DateTime.Now.Day}-{DateTime.Now.Hour}-{DateTime.Now.Minute}.txt")
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .CreateLogger();
        }
    }
}