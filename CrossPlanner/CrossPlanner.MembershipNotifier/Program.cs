using CrossPlanner.Domain.Context;
using CrossPlanner.Domain.Models;
using CrossPlanner.Repository.Wrapper;
using CrossPlanner.Service.Messages;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace CrossPlanner.MembershipNotifier
{
    public class Program
    {
        private static IConfiguration configuration;
        private const int FirstReminderDays = 14;
        private const int FinalReminderDays = 7;

        public static async Task Main(string[] args)
        {
            ServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            SetUpLogger();

            Log.Information($"Started membership notifier - {DateTime.Now}");

            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            try
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var repositoryWrapper = scope.ServiceProvider.GetRequiredService<IRepositoryWrapper>();
                    var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
                    await SendMembershipNotificationAsync(repositoryWrapper, emailSender);
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

        private static async Task SendMembershipNotificationAsync(IRepositoryWrapper repositoryWrapper, IEmailSender emailSender)
        {
            var dateTimeNow = DateTime.Now;
            var firstReminderDate = dateTimeNow.AddDays(FirstReminderDays);
            var finalReminderDate = dateTimeNow.AddDays(FinalReminderDays);
            
            var membershipsForFirstReminder = await repositoryWrapper.MembershipRepository
                            .FindByCondition(m => m.EndDate.HasValue
                            && m.EndDate.Value.Date == firstReminderDate.Date
                            && m.IsActive)
                            .Include(m => m.Member)
                            .Include(m => m.MembershipPlan)
                            .ToListAsync();

            var membershipsForFinalReminder = await repositoryWrapper.MembershipRepository
                .FindByCondition(m => m.EndDate.HasValue
                && m.EndDate.Value.Date == finalReminderDate.Date
                && m.IsActive)
                .Include(m => m.Member)
                .ToListAsync();

            foreach (var membership in membershipsForFirstReminder)
            {
                string subject = membership.AutoRenew ? "Upcoming Membership Renewal" : "Upcoming Membership Expiration";
                string message = membership.AutoRenew
                    ? "Your membership will auto-renew soon."
                    : "Your membership will expire soon.";
                await SendReminderEmailAsync(emailSender, membership, subject, message);
            }

            foreach (var membership in membershipsForFinalReminder)
            {
                string subject = membership.AutoRenew ? "Final Reminder for Membership Renewal" : "Final Reminder for Membership Expiration";
                string message = membership.AutoRenew
                    ? "Your membership is set to auto-renew soon."
                    : "Your membership will expire soon.";
                await SendReminderEmailAsync(emailSender, membership, subject, message);
            }
        }

        private static async Task SendReminderEmailAsync(IEmailSender emailSender, Membership membership, string subject, string message)
        {
            if (membership.Member == null || string.IsNullOrEmpty(membership.Member.Email))
            {
                Log.Error($"Failed to send email: No email found for user {membership.MemberId}");
                return;
            }

            var emailBody = $"{message}\nMembership Plan: {membership.MembershipPlanId}\nEnd Date: {membership.EndDate?.ToShortDateString()}";

            try
            {
                await emailSender.SendEmailAsync(membership.Member.Email, subject, emailBody);
                Log.Information($"Sent {subject} to user {membership.MemberId} {membership.Member.Email}");
            }
            catch(Exception ex)
            {
                Log.Error($"Error sending email to user {membership.MemberId} at {membership.Member.Email}: {ex.Message}");
            }
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

            serviceCollection.AddDbContext<ApplicationContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("CrossPlannerConnection")));

            serviceCollection.AddSingleton<IRepositoryWrapper, RepositoryWrapper>();
            serviceCollection.AddSingleton<IEmailSender, EmailSender>();
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